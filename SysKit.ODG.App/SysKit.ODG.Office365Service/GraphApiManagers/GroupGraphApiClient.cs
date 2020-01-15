extern alias BetaLib;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Graph;
using Newtonsoft.Json;
using Serilog;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;
using SysKit.ODG.Base.Office365;
using SysKit.ODG.Office365Service.GraphHttpProvider;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;
using Beta = BetaLib.Microsoft.Graph;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public class GroupGraphApiClient: BaseGraphApiClient, IGroupGraphApiClient
    {
        private readonly INotifier _notifier;
        public GroupGraphApiClient(IAccessTokenManager accessTokenManager,
            INotifier notifier,
            IGraphHttpProviderFactory graphHttpProviderFactory,
            IGraphServiceFactory graphServiceFactory,
            IMapper autoMapper) : base(accessTokenManager, graphHttpProviderFactory, graphServiceFactory, autoMapper)
        {
            _notifier = notifier;
        }

        /// <inheritdoc />
        public async Task<CreatedGroupsResult> CreateUnifiedGroups(IEnumerable<UnifiedGroupEntry> groups, UserEntryCollection users)
        {
            using var progressUpdater = new ProgressUpdater("Create Unified Groups", _notifier);
            var createdGroupsResult = new CreatedGroupsResult();
            var groupLookup = new Dictionary<string, UnifiedGroupEntry>();
            var batchEntries = new List<GraphBatchRequest>();

            int i = 0;
            foreach (var group in groups)
            {
                if (tryCreateGroupBatch(users, groupLookup, @group, createdGroupsResult, out var graphGroup))
                {
                    batchEntries.Add(new GraphBatchRequest(group.MailNickname, "groups", HttpMethod.Post, graphGroup));
                }
            }

            await executeActionWithProgress(progressUpdater, batchEntries, onResult: (key, value) =>
            {
                var originalGroup = groupLookup[key];
                if (value.IsSuccessStatusCode)
                {
                    var createdGroup = deserializeGraphObject<Group>(value.Content).GetAwaiter().GetResult();
                    originalGroup.GroupId = createdGroup.Id;
                    createdGroupsResult.AddGroup(originalGroup);
                }
                else
                {
                    if (isKnownError(GraphAPIKnownErrorMessages.GroupAlreadyExists, value))
                    {
                        _notifier.Warning($"Failed to create: {originalGroup.MailNickname}. Group already exists");
                    }
                    else
                    {
                        _notifier.Error($"Failed to create: {originalGroup.MailNickname}. {getErrorMessage(value)}");
                    }
                }
            });
            
            _notifier.Info($"Waiting for groups to provision");
            var failedGroupsCount = await waitForGroupProvisioning(createdGroupsResult.CreatedGroups.ToDictionary(g => g.GroupId, g => new GraphBatchRequest(g.MailNickname, $"groups/{g.GroupId}/drive",HttpMethod.Get)));
            _notifier.Info($"Group provisioning finished. Failed groups count: {failedGroupsCount}");

            return createdGroupsResult;
        }

        /// <inheritdoc />
        public async Task<List<TeamEntry>> CreateTeamsFromGroups(IEnumerable<TeamEntry> teams, UserEntryCollection users)
        {
            using var progressUpdater = new ProgressUpdater("Create Teams", _notifier);
            var successfullyCreatedTeams = new ConcurrentBag<TeamEntry>();
            var failedTeams = new ConcurrentBag<TeamEntry>();

            async Task executeCreateTeams(IEnumerable<TeamEntry> newTeams)
            {
                var teamLookup = new Dictionary<string, TeamEntry>();
                var batchEntries = new List<GraphBatchRequest>();
                foreach (var team in newTeams)
                {
                    if (tryCreateTeamBatch(users, teamLookup, team, out var graphTeam))
                    {
                        batchEntries.Add(new GraphBatchRequest(team.MailNickname, "teams", HttpMethod.Post, graphTeam));
                    }
                }

                await executeActionWithProgress(progressUpdater, batchEntries, onResult: (key, value) =>
                {
                    var originalTeam = teamLookup[key];
                    if (value.IsSuccessStatusCode)
                    {
                        successfullyCreatedTeams.Add(originalTeam);
                    }
                    else
                    {
                        // TODO: handle only if group id doesnt exist
                        failedTeams.Add(teamLookup[key]);
                        _notifier.Error($"Failed to create: {originalTeam.MailNickname} .{getErrorMessage(value)}");
                    }
                });
            }

            await executeCreateTeams(teams);

            int waitTime = 10;
            int attempts = 0;
            while (failedTeams.Any() && attempts < 3)
            {
                var toRepeat = failedTeams.ToList();
                failedTeams = new ConcurrentBag<TeamEntry>();
                // group provisioning was not finished, so lts wait and try again
                _notifier.Warning($"Retry team creation for {toRepeat.Count}, time: {waitTime}");
                await Task.Delay(TimeSpan.FromSeconds(waitTime));
                await executeCreateTeams(toRepeat);
                waitTime += 15;
                attempts++;
            }

            return successfullyCreatedTeams.ToList();
        }

        /// <inheritdoc />
        public async Task CreateTeamChannels(IEnumerable<TeamEntry> teams, UserEntryCollection users)
        {
            using var progressUpdater = new ProgressUpdater("Create Channels", _notifier);
            var batchEntries = new List<GraphBatchRequest>();
            var channelLookup = new Dictionary<string, TeamChannelEntry>();

            var i = 0;
            foreach (var team in teams)
            {
                if (team?.Channels?.Any() != true)
                {
                    continue;
                }

                foreach (var teamChannelEntry in team.Channels)
                {
                    if (tryCreateChannel(users, teamChannelEntry, out var graphChannel))
                    {
                        var requestKey = $"{++i}/{team.GroupId}";
                        channelLookup.Add(requestKey, teamChannelEntry);
                        batchEntries.Add(new GraphBatchRequest(requestKey, $"teams/{team.GroupId}/channels", HttpMethod.Post, graphChannel));
                    }
                }
            }

            await executeActionWithProgress(progressUpdater, batchEntries, onResult: (key, value) =>
            {
                var channelEntry = channelLookup[key];
                var teamId = key.Split('/')[1];
                if (!value.IsSuccessStatusCode)
                {
                    _notifier.Error($"Failed to create channel {channelEntry.DisplayName}(teamId: {teamId}). {getErrorMessage(value)}");
                }
            });
        }

        /// <inheritdoc />
        public async Task RemoveGroupOwners(Dictionary<string, UnifiedGroupEntry> ownersMap)
        {
            using var progressUpdater = new ProgressUpdater("Remove Group Owner", _notifier);
            var batchEntries = new List<GraphBatchRequest>();

            var i = 0;
            foreach (var ownerMap in ownersMap)
            {
                batchEntries.Add(new GraphBatchRequest($"{++i}", $"/groups/{ownerMap.Value.GroupId}/owners/{ownerMap.Key}/$ref", HttpMethod.Delete));
            }

            await executeActionWithProgress(progressUpdater, batchEntries, onResult: (key, value) =>
            {
                if (!value.IsSuccessStatusCode)
                {
                    _notifier.Error($"Failed to remove group owner. {getErrorMessage(value)}");
                }
            });
        }

        #region Helpers

        private int _maxProvisioningAttempts = 3;
        /// <summary>
        /// Waits for Group drive to be available. This is a sign that group was provisioned
        /// </summary>
        /// <param name="groupDriveBatchRequests"></param>
        /// <param name="attempt"></param>
        /// <returns></returns>
        private async Task<int> waitForGroupProvisioning(Dictionary<string, GraphBatchRequest> groupDriveBatchRequests, int attempt = 0)
        {
            if (attempt >= _maxProvisioningAttempts || !groupDriveBatchRequests.Any())
            {
                return groupDriveBatchRequests.Count;
            }

            await Task.Delay(TimeSpan.FromSeconds(attempt * 15));

            var failedGroups = new Dictionary<string, GraphBatchRequest>();
            var groupDriveResults = await _httpProvider.SendBatchAsync(groupDriveBatchRequests.Values, _accessTokenManager, false);
            foreach (var result in groupDriveResults)
            {
                if (!result.Value.IsSuccessStatusCode)
                {
                    failedGroups.Add(result.Key, groupDriveBatchRequests[result.Key]);
                }
            }

            return await waitForGroupProvisioning(failedGroups, attempt + 1);
        }

        private bool tryCreateGroupBatch(UserEntryCollection users, Dictionary<string, UnifiedGroupEntry> groupLookup, UnifiedGroupEntry @group, CreatedGroupsResult createResult, out GroupExtended graphGroup)
        {
            graphGroup = null;
            if (groupLookup.ContainsKey(@group.MailNickname))
            {
                _notifier.Warning( $"Trying to create 2 groups with same mail nickname ({@group.MailNickname}). Only the first will be created.");
                return false;
            }

            groupLookup.Add(@group.MailNickname, @group);
            graphGroup = new GroupExtended
            {
                DisplayName = @group.DisplayName,
                MailNickname = @group.MailNickname,
                Visibility = @group.IsPrivate ? "Private" : "Public",
                MailEnabled = true,
                SecurityEnabled = false,
                GroupTypes = new List<string> {"Unified"}
            };

            return tryCreateMembersOwnersBindings(group, graphGroup, users, createResult);
        }

        private bool tryCreateTeamBatch(UserEntryCollection users, Dictionary<string, TeamEntry> teamLookup, TeamEntry team, out TeamExtended graphTeam)
        {
            graphTeam = null;
            if (teamLookup.ContainsKey(team.MailNickname))
            {
                _notifier.Warning($"Trying to create 2 teams with same mail nickname ({team.MailNickname}). Only the first will be created.");
                return false;
            }

            teamLookup.Add(team.MailNickname, team);
            graphTeam = new TeamExtended(team.GroupId);

            //if (team?.Channels.Any() == true)
            //{
            //    graphTeam.Channels = new Beta.TeamChannelsCollectionPage();
            //    // TODO: private channels are not supported currently. This will hopefully change :)
            //    foreach (var channel in team.Channels.Where(c => !c.IsPrivate))
            //    {
            //        if (!tryCreateChannel(users, channel, out var newChannel)) return false;
            //        graphTeam.Channels.Add(newChannel);
            //    }
            //}

            return true;
        }

        private bool tryCreateChannel(UserEntryCollection users, TeamChannelEntry channel, out Beta.Channel newChannel)
        {
            newChannel = new Beta.Channel()
            {
                DisplayName = channel.DisplayName,
                MembershipType = channel.IsPrivate
                    ? Beta.ChannelMembershipType.Private
                    : Beta.ChannelMembershipType.Standard
            };

            if (channel.IsPrivate)
            {
                var channelMembers = new Beta.ChannelMembersCollectionPage();

                if (!tryCreateChannelMemberCollection(users, channel.Owners, "owner", out var owners)) return false;
                owners.ForEach(o => channelMembers.Add(o));

                if (!tryCreateChannelMemberCollection(users, channel.Members, "member", out var members)) return false;
                members.ForEach(o => channelMembers.Add(o));

                newChannel.Members = channelMembers;
            }

            return true;
        }

        private bool tryCreateChannelMemberCollection(UserEntryCollection users, IEnumerable<MemberEntry> memberEntries, string role, out List<Beta.AadUserConversationMember> members)
        {
            members = new List<Beta.AadUserConversationMember>();

            if (memberEntries == null)
            {
                // we do nothing
                return true;
            }

            foreach (var member in memberEntries)
            {
                var memberEntry = users.FindMember(member);
                if (memberEntry == null)
                {
                    // we want all or nothing
                    _notifier.Warning($"{role} not found ({member.Name}).");
                    return false;
                }

                members.Add(new Beta.AadUserConversationMember
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"user@odata.bind", $"https://graph.microsoft.com/beta/users('{memberEntry.Id}')"}
                    },
                    Roles = new List<String>()
                    {
                        role
                    }
                });
            }

            return true;
        }

        /// <summary>
        /// Sets owners/members to graph group. If any owner/member is missing it will return false
        /// </summary>
        /// <param name="group"></param>
        /// <param name="graphGroup"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        private bool tryCreateMembersOwnersBindings(UnifiedGroupEntry @group, IGroupMembership graphGroup, UserEntryCollection users, CreatedGroupsResult createResult)
        {
            if (@group.Owners?.Any() == true)
            {
                var userIds = new HashSet<string>();
                foreach (var owner in @group.Owners)
                {
                    var ownerEntry = users.FindMember(owner);

                    if (ownerEntry == null)
                    {
                        // we want all or nothing
                        _notifier.Warning($"Failed to create group: {@group.MailNickname}. Owner not found: {owner.Name}");
                        return false;
                    }
                    else
                    {
                        userIds.Add(ownerEntry.Id);
                    }
                }

                var currentUserUsername = _accessTokenManager.GetUsernameFromToken();
                var currentUserEntry = users.FindMember(new MemberEntry(currentUserUsername));
                if (currentUserEntry != null && !userIds.Contains(currentUserEntry.Id))
                {
                    userIds.Add(currentUserEntry.Id);
                    createResult.AddGroupWhereOwnerWasAdded(currentUserEntry.Id, group);
                }

                if (userIds.Any())
                {
                    graphGroup.OwnersODataBind =
                        userIds.Select(id => $"https://graph.microsoft.com/v1.0/users/{id}").ToArray();
                }
            }
            
            if (@group.Members?.Any() == true)
            {
                var userIds = new HashSet<string>();
                foreach (var member in @group.Members)
                {
                    var memberEntry = users.FindMember(member);

                    if (memberEntry == null)
                    {
                        // we want all or nothing
                        _notifier.Warning($"Failed to create group: {@group.MailNickname}. Member not found: {member.Name}");
                        return false;
                    }
                    else
                    {
                        userIds.Add(memberEntry.Id);
                    }
                }

                if (userIds.Any())
                {
                    graphGroup.MembersODataBind =
                        userIds.Select(id => $"https://graph.microsoft.com/v1.0/users/{id}").ToArray();
                }
            }

            return true;
        }

        interface IGroupMembership
        {
            string[] OwnersODataBind { get; set; }
            string[] MembersODataBind { get; set; }
        }

        class GroupExtended : Group, IGroupMembership
        {
            [JsonProperty("owners@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
            public string[] OwnersODataBind { get; set; }
            [JsonProperty("members@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
            public string[] MembersODataBind { get; set; }
        }

        class TeamExtended: Beta.Team
        {
            [JsonProperty("group@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
            public string GroupBind { get; }

            [JsonProperty("template@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
            public string TemplateBind => "https://graph.microsoft.com/beta/teamsTemplates('standard')";

            public TeamExtended(string groupId)
            {
                GroupBind = $"https://graph.microsoft.com/v1.0/groups('{groupId}')";
            }
        }

        #endregion Helpers
    }
}

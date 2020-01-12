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
using SysKit.ODG.Base.Office365;
using SysKit.ODG.Office365Service.GraphHttpProvider;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;
using Beta = BetaLib.Microsoft.Graph;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public class GroupGraphApiClient: BaseGraphApiClient, IGroupGraphApiClient
    {
        private readonly ILogger _logger;
        public GroupGraphApiClient(IAccessTokenManager accessTokenManager,
            ILogger logger,
            IGraphHttpProviderFactory graphHttpProviderFactory,
            IGraphServiceFactory graphServiceFactory,
            IMapper autoMapper) : base(accessTokenManager, graphHttpProviderFactory, graphServiceFactory, autoMapper)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<CreatedGroupsResult> CreateUnifiedGroups(IEnumerable<UnifiedGroupEntry> groups, UserEntryCollection users)
        {
            var createdGroupsResult = new CreatedGroupsResult();
            int groupsProcessed = 0;
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

            if (!batchEntries.Any())
            {
                return new CreatedGroupsResult();
            }

            Action<Dictionary<string, HttpResponseMessage>> handleBatchResult = results =>
            {
                foreach (var result in results)
                {
                    if (result.Value.IsSuccessStatusCode)
                    {
                        var originalGroup = groupLookup[result.Key];
                        var createdGroup = DeserializeGraphObject<Group>(result.Value.Content).GetAwaiter().GetResult();

                        originalGroup.GroupId = createdGroup.Id;
                        createdGroupsResult.AddGroup(originalGroup);
                    }
                    else
                    {
                        _logger.Warning($"Failed to create group: {result.Key}. Status code: {(int)result.Value.StatusCode}");
                    }

                    result.Value.Dispose();
                }

                Interlocked.Add(ref groupsProcessed, results.Count);
                _logger.Information($"Groups processed: {groupsProcessed}/{batchEntries.Count}");
            };

            await _httpProvider.StreamBatchAsync(batchEntries, _accessTokenManager, handleBatchResult);
            return createdGroupsResult;
        }

        /// <inheritdoc />
        public async Task<List<TeamEntry>> CreateTeamsFromGroups(IEnumerable<TeamEntry> teams, UserEntryCollection users)
        {
            var successfullyCreatedTeams = new ConcurrentBag<TeamEntry>();
            var failedTeams = new ConcurrentBag<TeamEntry>();

            async Task executeCreateTeams(IEnumerable<TeamEntry> newTeams)
            {
                int teamsProcessed = 0;
                var teamLookup = new Dictionary<string, TeamEntry>();
                var batchEntries = new List<GraphBatchRequest>();
                foreach (var team in newTeams)
                {
                    if (tryCreateTeamBatch(users, teamLookup, team, out var graphTeam))
                    {
                        batchEntries.Add(new GraphBatchRequest(team.MailNickname, "teams", HttpMethod.Post, graphTeam));
                    }
                }

                if (!batchEntries.Any())
                {
                    return;
                }

                Action<Dictionary<string, HttpResponseMessage>> handleBatchResult = results =>
                {
                    foreach (var result in results)
                    {
                        if (result.Value.IsSuccessStatusCode)
                        {
                            var originalTeam = teamLookup[result.Key];
                            successfullyCreatedTeams.Add(originalTeam);
                        }
                        else
                        {
                            // TODO: handle only if group id doesnt exist
                            failedTeams.Add(teamLookup[result.Key]);
                            _logger.Warning(
                                $"Failed to create team: {result.Key}. Status code: {(int) result.Value.StatusCode}");
                        }

                        result.Value.Dispose();
                    }

                    Interlocked.Add(ref teamsProcessed, results.Count);
                    _logger.Information($"Teams processed: {teamsProcessed}/{newTeams.Count()}");
                };

                await _httpProvider.StreamBatchAsync(batchEntries, _accessTokenManager, handleBatchResult, true);
            }

            await executeCreateTeams(teams);

            int waitTime = 10;
            int attempts = 0;
            while (failedTeams.Any() || attempts < 3)
            {
                var toRepeat = failedTeams.ToList();
                failedTeams = new ConcurrentBag<TeamEntry>();
                // group provisioning was not finished, so lts wait and try again
                _logger.Information($"Retry team creation for {toRepeat.Count}, time: {waitTime}");
                await Task.Delay(TimeSpan.FromSeconds(waitTime));
                await executeCreateTeams(toRepeat);
                waitTime += 15;
                attempts++;
            }

            return successfullyCreatedTeams.ToList();
        }

        /// <inheritdoc />
        public async Task CreatePrivateChannels(IEnumerable<TeamEntry> teams, UserEntryCollection users)
        {
            var batchEntries = new List<GraphBatchRequest>();

            var i = 0;
            foreach (var team in teams)
            {
                if (team?.Channels?.Any() != true)
                {
                    continue;
                }

                foreach (var teamChannelEntry in team.Channels?.Where(c => c.IsPrivate))
                {
                    if (tryCreateChannel(users, teamChannelEntry, out var graphChannel))
                    {
                        batchEntries.Add(new GraphBatchRequest($"{++i}", $"teams/{team.GroupId}/channels", HttpMethod.Post, graphChannel));
                    }
                }
            }

            var results = await _httpProvider.SendBatchAsync(batchEntries, _accessTokenManager, true);
            foreach (var result in results)
            {
                if (!result.Value.IsSuccessStatusCode)
                {
                    // TODO: better handling
                    _logger.Warning("Failed to create channel");
                }
            }
        }

        /// <inheritdoc />
        public async Task RemoveGroupOwners(Dictionary<string, UnifiedGroupEntry> ownersMap)
        {
            var batchEntries = new List<GraphBatchRequest>();

            var i = 0;
            foreach (var ownerMap in ownersMap)
            {
                batchEntries.Add(new GraphBatchRequest($"{++i}", $"/groups/{ownerMap.Value.GroupId}/owners/{ownerMap.Key}/$ref", HttpMethod.Delete));
            }

            var results = await _httpProvider.SendBatchAsync(batchEntries, _accessTokenManager);
            foreach (var result in results)
            {
                if (!result.Value.IsSuccessStatusCode)
                {
                    // TODO: better handling
                    _logger.Warning("Failed to remove owner");
                }
            }
        }

        #region Helpers

        private bool tryCreateGroupBatch(UserEntryCollection users, Dictionary<string, UnifiedGroupEntry> groupLookup, UnifiedGroupEntry @group, CreatedGroupsResult createResult, out GroupExtended graphGroup)
        {
            graphGroup = null;
            if (groupLookup.ContainsKey(@group.MailNickname))
            {
                _logger.Warning(
                    $"Trying to create 2 groups with same mail nickname ({@group.MailNickname}). Only the first will be created.");
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
                _logger.Warning(
                    $"Trying to create 2 teams with same mail nickname ({team.MailNickname}). Only the first will be created.");
                return false;
            }

            teamLookup.Add(team.MailNickname, team);
            graphTeam = new TeamExtended(team.GroupId);

            if (team?.Channels.Any() == true)
            {
                graphTeam.Channels = new Beta.TeamChannelsCollectionPage();
                // TODO: private channels are not supported currently. This will hopefully change :)
                foreach (var channel in team.Channels.Where(c => !c.IsPrivate))
                {
                    if (!tryCreateChannel(users, channel, out var newChannel)) return false;
                    graphTeam.Channels.Add(newChannel);
                }
            }

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
                    _logger.Warning($"Failed to create team. User not found: {member.Name}");
                    // we want all or nothing
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
                        _logger.Warning($"Failed to create group: {@group.MailNickname}. Owner not found: {owner.Name}");
                        // we want all or nothing
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
                        _logger.Warning($"Failed to create group: {@group.MailNickname}. Member not found: {member.Name}");
                        // we want all or nothing
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

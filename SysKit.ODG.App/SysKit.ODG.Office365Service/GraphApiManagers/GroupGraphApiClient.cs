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
        public async Task<List<UnifiedGroupEntry>> CreateUnifiedGroups(IEnumerable<UnifiedGroupEntry> groups, UserEntryCollection users)
        {
            int groupsProcessed = 0;
            var groupLookup = new Dictionary<string, UnifiedGroupEntry>();
            var successfullyCreatedGroups = new ConcurrentBag<UnifiedGroupEntry>();
            var batchEntries = new List<GraphBatchRequest>();

            int i = 0;
            foreach (var group in groups)
            {
                if (tryCreateGroupBatch(users, groupLookup, @group, out var graphGroup))
                {
                    batchEntries.Add(new GraphBatchRequest(group.MailNickname, "groups", HttpMethod.Post, graphGroup));
                }
            }

            if (!batchEntries.Any())
            {
                return new List<UnifiedGroupEntry>();
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
                        successfullyCreatedGroups.Add(originalGroup);
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
            return successfullyCreatedGroups.ToList();
        }

        public async Task<List<TeamEntry>> CreateTeams(IEnumerable<TeamEntry> teams, UserEntryCollection users)
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

            int waitTime = 30;
            while (failedTeams.Any())
            {
                var toRepeat = failedTeams.ToList();
                failedTeams = new ConcurrentBag<TeamEntry>();
                // group provisioning was not finished, so lts wait and try again
                _logger.Information($"Retry team creation for {toRepeat.Count}, time: {waitTime}");
                await Task.Delay(TimeSpan.FromSeconds(30));
                await executeCreateTeams(toRepeat);
                waitTime += 15;
            }

            return successfullyCreatedTeams.ToList();
        }

        #region Helpers

        private bool tryCreateGroupBatch(UserEntryCollection users, Dictionary<string, UnifiedGroupEntry> groupLookup, UnifiedGroupEntry @group, out GroupExtended graphGroup)
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

            return tryCreateMembersOwnersBindings(group, graphGroup, users);
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
                foreach (var channel in team.Channels)
                {
                    var newChannel = new Beta.Channel()
                    {
                        DisplayName = channel.DisplayName,
                        MembershipType = channel.IsPrivate
                            ? Beta.ChannelMembershipType.Private
                            : Beta.ChannelMembershipType.Standard
                    };

                    if (channel.IsPrivate)
                    {
                        newChannel.Members = new Beta.ChannelMembersCollectionPage();
                        foreach (var member in channel.Owners)
                        {
                            var memberEntry = users.FindMember(member);
                            if (memberEntry == null)
                            {
                                _logger.Warning($"Failed to create team: {team.MailNickname}. User not found: {member.Name}");
                                // we want all or nothing
                                return false;
                            }

                            newChannel.Members.Add(new Beta.AadUserConversationMember
                            {
                                AdditionalData = new Dictionary<string, object>()
                                {
                                    {"user@odata.bind",$"https://graph.microsoft.com/beta/users/{memberEntry.Id}"}
                                },
                                Roles = new List<String>()
                                {
                                    "owner"
                                }
                            });
                        }

                        //foreach (var member in channel.Members)
                        //{
                        //    var memberEntry = users.FindMember(member);
                        //    if (memberEntry == null)
                        //    {
                        //        _logger.Warning($"Failed to create team: {team.MailNickname}. User not found: {member.Name}");
                        //        // we want all or nothing
                        //        return false;
                        //    }

                        //    newChannel.Members.Add(new Beta.AadUserConversationMember
                        //    {
                        //        AdditionalData = new Dictionary<string, object>()
                        //        {
                        //            {"user@odata.bind",$"https://graph.microsoft.com/beta/users/{memberEntry.Id}"}
                        //        },
                        //        Roles = new List<String>()
                        //        {
                        //            "member"
                        //        }
                        //    });
                        //}
                    }

                    graphTeam.Channels.Add(newChannel);
                }
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
        private bool tryCreateMembersOwnersBindings(UnifiedGroupEntry @group, IGroupMembership graphGroup, UserEntryCollection users)
        {
            //if (@group.Owners?.Any() == true)
            //{
            //    var userIds = new HashSet<string>();
            //    foreach (var owner in @group.Owners)
            //    {
            //        var ownerEntry = users.FindMember(owner);

            //        if (ownerEntry == null)
            //        {
            //            _logger.Warning($"Failed to create group: {@group.MailNickname}. Owner not found: {owner.Name}");
            //            // we want all or nothing
            //            return false;
            //        }
            //        else
            //        {
            //            userIds.Add(ownerEntry.Id);
            //        }
            //    }

            //    if (userIds.Any())
            //    {
            //        graphGroup.OwnersODataBind =
            //            userIds.Select(id => $"https://graph.microsoft.com/v1.0/users/{id}").ToArray();
            //    }
            //}

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

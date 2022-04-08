extern alias BetaLib;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeDevPnP.Core.Framework.Graph;
using SysKit.ODG.Base.DTO;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;
using SysKit.ODG.Base.Office365;
using SysKit.ODG.Office365Service.GraphHttpProvider;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;
using Beta = BetaLib.Microsoft.Graph;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public class GroupGraphApiClient : BaseGraphApiClient, IGroupGraphApiClient
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
            var groupsWithTooManyMembers = new HashSet<UnifiedGroupEntry>();
            var groupList = groups.ToList();

            progressUpdater.SetTotalCount(groupList.Count);
            foreach (var group in groupList)
            {
                try
                {
                    var graphGroup = createGraphGroup(users, groupLookup, @group, createdGroupsResult);
                    if (graphGroup.HasTooManyMembers())
                    {
                        groupsWithTooManyMembers.Add(group);
                        graphGroup.MembersODataBind = null;
                        graphGroup.OwnersODataBind = null;
                    }

                    var createdGroup = await _graphServiceClient.Groups.Request().AddAsync(graphGroup);
                    group.GroupId = createdGroup.Id;

                    createdGroupsResult.AddGroup(group);

                    group.SiteUrl = await waitForGroupProvisioning(group.GroupId);
                }
                catch (ServiceException sex)
                {
                    var value = sex.Error;
                    if (isKnownError(GraphAPIKnownErrorMessages.GuestUserGroupOwnerError, value))
                    {
                        // this should not really happen
                        var owners = string.Join(",", group.Owners.Select(o => o.Name));
                        _notifier.Error(
                            $"Failed to create: {group.MailNickname}. Guest owner detected. Owners: {owners}");
                    }
                    else if (isKnownError(GraphAPIKnownErrorMessages.GroupAlreadyExists, value))
                    {
                        _notifier.Warning($"Failed to create: {group.MailNickname}. Group already exists");
                    }
                    else
                    {
                        _notifier.Error($"Failed to create: {group.MailNickname}. {value?.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _notifier.Error(ex.Message);
                }
                finally
                {
                    progressUpdater.UpdateProgress(1);
                }
            }

            progressUpdater.Flush();
            createdGroupsResult.RemoveGroupsByGroupId(await addGroupMemberships(createdGroupsResult.FilterOnlyCreatedGroups(groupsWithTooManyMembers).Cast<GroupEntry>().ToList(), users));
            createdGroupsResult.HasErrors = groupList.Count != createdGroupsResult.CreatedGroups.Count;
            return createdGroupsResult;
        }

        /// <inheritdoc />
        public async Task<O365CreationResult<TeamEntry>> CreateTeamsFromGroups(IEnumerable<TeamEntry> teams,
            UserEntryCollection users)
        {
            using var progressUpdater = new ProgressUpdater("Create Teams", _notifier);
            var successfullyCreatedTeams = new List<TeamEntry>();
            var failedTeams = new List<TeamEntry>();
            var teamLookup = new Dictionary<string, TeamEntry>();
            var teamList = teams.ToList();

            var maxRetryAttempt = 5;
            var waitPeriod = 15;

            async Task<bool> tryCreateTeam(TeamEntry newTeam, int retryAttempt = 0)
            {
                // failed every time
                if (retryAttempt > maxRetryAttempt)
                {
                    _notifier.Error($"Failed to create: {newTeam.MailNickname}");
                    return false;
                }

                await Task.Delay(TimeSpan.FromSeconds(waitPeriod * retryAttempt));

                try
                {
                    var graphTeam = createGraphTeam(users, teamLookup, newTeam);
                    var requestUrl = HttpUtils.CreateGraphUrl("teams", true);
                    var createdTeam = await _httpProvider.SendAsync(await HttpUtils.CreateRequest(requestUrl, HttpMethod.Post, _accessTokenManager, graphTeam));

                    if (createdTeam.IsSuccessStatusCode)
                    {
                        // wait for team to provision
                        var teamProvisioned = await waitForTeamProvisioning(newTeam.GroupId);

                        return teamProvisioned;
                    }

                    if (!isKnownError(HttpStatusCode.NotFound, createdTeam) && !isKnownError(HttpStatusCode.BadGateway, createdTeam))
                    {
                        _notifier.Error($"Failed to create: {newTeam.MailNickname} .{getErrorMessage(createdTeam)}. Attempt: {retryAttempt}");
                    }
                }
                catch (Exception ex)
                {
                    _notifier.Error(ex.Message);
                }

                return await tryCreateTeam(newTeam, retryAttempt + 1);
            }

            progressUpdater.SetTotalCount(teamList.Count);
            foreach (var team in teamList)
            {
                var teamCreated = await tryCreateTeam(team);
                progressUpdater.UpdateProgress(1);

                if (teamCreated)
                {
                    successfullyCreatedTeams.Add(team);
                }
                else
                {
                    failedTeams.Add(team);
                }
            }

            progressUpdater.Flush();
            return new O365CreationResult<TeamEntry>(successfullyCreatedTeams, successfullyCreatedTeams.Count != teamList.Count);
        }

        /// <inheritdoc />
        public async Task<List<TeamChannelResult>> CreatePrivateTeamChannels(IEnumerable<PrivateTeamChannelCreationOptions> channels)
        {
            using var progressUpdater = new ProgressUpdater("Create Private Channels", _notifier);
            var batchEntries = new List<GraphBatchRequest>();
            var channelLookup = new Dictionary<string, PrivateTeamChannelCreationOptions>();
            var failedChannels = new ConcurrentBag<PrivateTeamChannelCreationOptions>();
            var createdChannels = new ConcurrentBag<TeamChannelResult>();

            var i = 0;

            foreach (var channel in channels)
            {

                try
                {
                    var graphChannel = createPrivateGraphChannel(channel.MemberIds, channel.OwnerIds, channel.ChannelName);
                    var requestKey = $"{++i}/{channel.GroupId}";
                    channelLookup.Add(requestKey, channel);
                    batchEntries.Add(new GraphBatchRequest(requestKey, $"teams/{channel.GroupId}/channels",
                        HttpMethod.Post, graphChannel));
                }
                catch (Exception ex)
                {
                    _notifier.Error(ex.Message);
                }
            }

            await executeActionWithProgress(progressUpdater, batchEntries, true, onResult: (key, value) =>
            {
                var channelEntry = channelLookup[key];
                var teamId = key.Split('/')[1];
                if (!value.IsSuccessStatusCode)
                {
                    _notifier.Error($"Failed to create Private Channel {channelEntry.ChannelName}(teamId: {teamId}). {getErrorMessage(value)}");
                    failedChannels.Add(channelEntry);
                }
                else
                {
                    var content = value.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    var channelId = JsonConvert.DeserializeObject<Channel>(content).Id;
                    createdChannels.Add(new TeamChannelResult() { GroupId = teamId, ChannelId = channelId });
                }
            }, 1);

            return createdChannels.ToList();
        }

        /// <inheritdoc />
        public async Task<List<string>> GetAllTenantTeamIds()
        {
            var teamIdRequest = _graphServiceBetaClient.Groups.Request().Filter("resourceProvisioningOptions/Any(x:x eq 'Team')").Top(999);
            var teamIds = new List<string>();
            do
            {
                var teams = await teamIdRequest.GetAsync();
                teamIds.AddRange(teams.Select(t => t.Id));
                teamIdRequest = teams.NextPageRequest;

            } while (teamIdRequest != null);

            return teamIds;
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, List<string>>> GetTeamMembers(List<string> groupIds)
        {
            using var progressUpdater = new ProgressUpdater("Get team members for private channels", _notifier);
            var membersLookup = new Dictionary<string, List<string>>();

            var batchEntries = groupIds.Select(groupId => new GraphBatchRequest(groupId, $"/teams/{groupId}/members", HttpMethod.Get)).ToList();

            await executeActionWithProgress(progressUpdater, batchEntries, true, onResult: (key, value) =>
             {
                 if (!value.IsSuccessStatusCode)
                 {
                     _notifier.Error($"Failed to get Team members for team id {key}. {getErrorMessage(value)}");
                 }
                 else
                 {
                     var content = value.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                     var obj = JObject.Parse(content);

                     var members = JsonConvert.DeserializeObject<List<Beta.AadUserConversationMember>>(obj["value"].ToString());
                     membersLookup[key] = members.Select(m => m.UserId).ToList();
                 }

             }, 1);

            return membersLookup;
        }

        public async Task ProvisionPrivateChannelSites(List<TeamChannelResult> privateChannelCreation)
        {
            var generalChannelIds = new ConcurrentBag<Tuple<string, string>>();
            using (var progressUpdater = new ProgressUpdater("Get General channel ids", _notifier))
            {
                var batchEntries = privateChannelCreation.Select(channel => new GraphBatchRequest(channel.GroupId,
                        $"/teams/{channel.GroupId}/channels?$filter=displayName eq 'General'&$select=id",
                        HttpMethod.Get))
                    .ToList();
                await executeActionWithProgress(progressUpdater, batchEntries, true, onResult: (key, value) =>
                {
                    if (!value.IsSuccessStatusCode)
                    {
                        _notifier.Error(
                            $"Failed to get General channel id for team with id {key}. {getErrorMessage(value)}");
                    }
                    else
                    {
                        var content = value.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var obj = JObject.Parse(content);

                        var channelIds = JsonConvert.DeserializeObject<List<Channel>>(obj["value"].ToString())
                            .Select(c => c.Id);
                        foreach (var channelId in channelIds)
                        {
                            generalChannelIds.Add((new Tuple<string, string>(key, channelId)));
                        }
                    }

                }, 1);
            }

            using (var progressUpdater2 = new ProgressUpdater("Enable PC site provisioning on team", _notifier))
            {
                var generalChannelBatchEntries = generalChannelIds.Select(generalId =>
                    new GraphBatchRequest(generalId.Item1,
                        $"/teams/{generalId.Item1}/channels/{generalId.Item2}/filesFolder", HttpMethod.Get)).ToList();
                await executeActionWithProgress(progressUpdater2, generalChannelBatchEntries, true,
                    onResult: (key, value) =>
                    {
                        if (!value.IsSuccessStatusCode)
                        {
                            _notifier.Error(
                                $"Failed to enable PC site provisioning for team with id {key}. {getErrorMessage(value)}");
                        }

                    }, 1);
            }

            await Task.Delay(TimeSpan.FromSeconds(30));

            using (var progressUpdater4 = new ProgressUpdater("Provision PC site", _notifier))
            {
                var i = 0;
                var privateChannelCreationBatchEntries = privateChannelCreation.Select(channel => new GraphBatchRequest($"{++i}/{channel.GroupId}", $"/teams/{channel.GroupId}/channels/{channel.ChannelId}/filesFolder", HttpMethod.Get)).ToList();
                await executeActionWithProgress(progressUpdater4, privateChannelCreationBatchEntries, true, onResult: (key, value) =>
                {
                    if (!value.IsSuccessStatusCode)
                    {
                        var teamId = key.Split('/')[1];
                        _notifier.Error($"Failed to Provision PC site for team {teamId}. {getErrorMessage(value)}");
                    }

                }, 1);
            }

        }

        /// <inheritdoc />
        public async Task<bool> RemoveGroupOwners(Dictionary<string, UnifiedGroupEntry> ownersMap)
        {
            using var progressUpdater = new ProgressUpdater("Remove Group Owner", _notifier);
            var batchEntries = new List<GraphBatchRequest>();
            var hasError = false;

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
                    hasError = true;
                }
            });

            return !hasError;
        }

        /// <inheritdoc />
        public Task<bool> DeleteUnifiedGroup(string groupId)
        {
            bool softDeleted = false;
            async Task<bool> deleteGroup(int attempt = 1)
            {
                if (attempt >= 5)
                {
                    return false;
                }

                if (attempt > 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 10));
                }

                try
                {
                    if (softDeleted == false)
                    {
                        UnifiedGroupsUtility.DeleteUnifiedGroup(groupId, (await _accessTokenManager.GetGraphToken()).Token);
                        softDeleted = true;
                    }

                    UnifiedGroupsUtility.PermanentlyDeleteUnifiedGroup(groupId, (await _accessTokenManager.GetGraphToken()).Token);
                    return true;
                }
                catch (Exception ex)
                {
                    _notifier.Error($"Error while deleting group: {groupId}", ex);
                    return await deleteGroup(attempt + 1);
                }
            }

            return deleteGroup();
        }

        #region Helpers

        private int _maxProvisioningAttempts = 5;

        /// <summary>
        /// Waits for Group drive to be available. This is a sign that group was provisioned
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>Returns group URL if provision succeeded or null if it failed</returns>
        private async Task<string> waitForGroupProvisioning(string groupId, int retryAttempt = 0)
        {
            var waitPeriod = 15;
            if (retryAttempt > _maxProvisioningAttempts)
            {
                _notifier.Warning($"Failed to provision group: {groupId}");
                return null;
            }

            await Task.Delay(TimeSpan.FromSeconds(waitPeriod * retryAttempt));

            try
            {
                return UnifiedGroupsUtility.GetUnifiedGroupSiteUrl(groupId,
                    (await _accessTokenManager.GetGraphToken()).Token);
            }
            catch
            {
                // provisioning failed
            }

            return await waitForGroupProvisioning(groupId, retryAttempt + 1);
        }

        /// <summary>
        ///  Checks if team was provisioned
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="retryAttempt"></param>
        /// <returns></returns>
        private async Task<bool> waitForTeamProvisioning(string groupId, int retryAttempt = 0)
        {
            var waitPeriod = 15;
            if (retryAttempt > _maxProvisioningAttempts)
            {
                _notifier.Warning($"Failed to provision team from group: {groupId}");
                return false;
            }

            await Task.Delay(TimeSpan.FromSeconds(waitPeriod * retryAttempt));

            try
            {
                var requestUrl = HttpUtils.CreateGraphUrl($"teams/{groupId}", true);
                var teamProvisioned = await _httpProvider.SendAsync(await HttpUtils.CreateRequest(requestUrl, HttpMethod.Get, _accessTokenManager));

                if (teamProvisioned.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch
            {
                // do nothing
            }

            return await waitForTeamProvisioning(groupId, retryAttempt + 1);
        }

        private async Task<bool> waitForTeamMembershipsProvisioning(string groupId, int expectedMemberCount, int retryAttempt = 0)
        {
            var waitPeriod = 15;
            if (retryAttempt > _maxProvisioningAttempts)
            {
                _notifier.Warning($"Team membership provisioning timeout exceeded for group: {groupId}");
                return false;
            }

            await Task.Delay(TimeSpan.FromSeconds(waitPeriod * retryAttempt));

            try
            {
                var requestUrl = HttpUtils.CreateGraphUrl($"teams/{groupId}/members", true);
                var teamProvisioned = await _httpProvider.SendAsync(await HttpUtils.CreateRequest(requestUrl, HttpMethod.Get, _accessTokenManager));
                if (teamProvisioned.IsSuccessStatusCode)
                {
                    var content = await teamProvisioned.Content.ReadAsStringAsync();
                    var obj = JObject.Parse(content);
                    if (int.TryParse(obj["@odata.count"].ToString(), out var membershipCount) && membershipCount >= expectedMemberCount)
                    {
                        return true;
                    };
                }
            }
            catch
            {
                // do nothing
            }

            return await waitForTeamMembershipsProvisioning(groupId, expectedMemberCount, retryAttempt + 1);
        }

        /// <summary>
        /// Waits for Group drive to be available. This is a sign that group was provisioned
        /// </summary>
        /// <param name="groupDriveBatchRequests"></param>
        /// <param name="attempt"></param>
        /// <returns>Group groupIds that failed to provision</returns>
        private async Task<List<string>> waitForGroupProvisioning(Dictionary<string, GraphBatchRequest> groupDriveBatchRequests, int attempt = 0)
        {
            if (attempt >= _maxProvisioningAttempts || !groupDriveBatchRequests.Any())
            {
                return groupDriveBatchRequests.Keys.ToList();
            }

            TimeSpan delayTime;

            if (groupDriveBatchRequests.Count < 20)
            {
                // we need to wait less time if we didn't try to create too much groups
                delayTime = TimeSpan.FromSeconds(attempt * 2 * 10);
            }
            else
            {
                delayTime = TimeSpan.FromSeconds((attempt + 1) * 4 * 10);
            }

            _notifier.Info($"Group provision. Attempt: {attempt + 1}. Number: {groupDriveBatchRequests.Count}. Delay time: {delayTime.TotalSeconds}s");
            await Task.Delay(delayTime);

            var failedGroups = new Dictionary<string, GraphBatchRequest>();
            var groupDriveResults = await _httpProvider.SendBatchAsync(groupDriveBatchRequests.Values, _accessTokenManager, true);
            var isLastAttempt = (attempt + 1) >= _maxProvisioningAttempts;
            foreach (var result in groupDriveResults)
            {
                if (!result.Value.IsSuccessStatusCode)
                {
                    failedGroups.Add(result.Key, groupDriveBatchRequests[result.Key]);
                    // if it is last attempt we want the message
                    if (!isLastAttempt && !isKnownError(GraphAPIKnownErrorMessages.GroupProvisionError, result.Value) && !isKnownError(GraphAPIKnownErrorMessages.GroupProvisionError1, result.Value) && !isKnownError(HttpStatusCode.NotFound, result.Value))
                    {
                        _notifier.Error($"Failed to provision group: {result.Key}. {getErrorMessage(result.Value)}. Attempt: {attempt + 1}");
                    }
                }
            }

            return await waitForGroupProvisioning(failedGroups, attempt + 1);
        }

        /// <summary>
        /// Waits for Team to be available
        /// </summary>
        /// <param name="teamBatchRequests"></param>
        /// <param name="attempt"></param>
        /// <returns>Returns group ids of failed teams</returns>
        private async Task<List<string>> waitForTeamProvisioning(Dictionary<string, GraphBatchRequest> teamBatchRequests, int attempt = 0)
        {
            if (attempt >= _maxProvisioningAttempts || !teamBatchRequests.Any())
            {
                return teamBatchRequests.Keys.ToList();
            }

            _notifier.Info($"Team provision. Attempt: {attempt + 1}. Number: {teamBatchRequests.Count}");

            await Task.Delay(TimeSpan.FromSeconds((attempt + 1) * 15));

            var failedTeams = new Dictionary<string, GraphBatchRequest>();
            var teamResults = await _httpProvider.SendBatchAsync(teamBatchRequests.Values, _accessTokenManager, true);
            foreach (var result in teamResults)
            {
                if (!result.Value.IsSuccessStatusCode)
                {
                    failedTeams.Add(result.Key, teamBatchRequests[result.Key]);
                    if (!isKnownError(GraphAPIKnownErrorMessages.TeamProvisionError, result.Value) && !isKnownError(HttpStatusCode.NotFound, result.Value))
                    {
                        _notifier.Error(getErrorMessage(result.Value));
                    }
                }
            }

            return await waitForTeamProvisioning(failedTeams, attempt + 1);
        }

        private GroupExtended createGraphGroup(UserEntryCollection users, Dictionary<string, UnifiedGroupEntry> groupLookup, UnifiedGroupEntry @group, CreatedGroupsResult createResult)
        {
            if (groupLookup.ContainsKey(@group.MailNickname))
            {
                throw new ArgumentException($"Trying to create 2 groups with same mail nickname ({@group.MailNickname}). Only the first will be created.");
            }

            groupLookup.Add(@group.MailNickname, @group);
            var graphGroup = new GroupExtended
            {
                DisplayName = @group.DisplayName,
                MailNickname = @group.MailNickname,
                Visibility = @group.IsPrivate ? "Private" : "Public",
                MailEnabled = true,
                SecurityEnabled = false,
                GroupTypes = new List<string> { "Unified" },
                Description = "Sample group"
            };

            var groupMembership = getGroupMemberships(users, @group);

            if (groupMembership.MemberIds.Any())
            {
                graphGroup.MembersODataBind =
                    groupMembership.MemberIds.Select(id => $"https://graph.microsoft.com/v1.0/users/{id}").ToArray();
            }

            if (groupMembership.OwnerIds.Any())
            {
                graphGroup.OwnersODataBind =
                    groupMembership.OwnerIds.Select(id => $"https://graph.microsoft.com/v1.0/users/{id}").ToArray();
            }

            if (groupMembership.CurrentUserAddedToOwners)
            {
                createResult.AddGroupWhereOwnerWasAdded(groupMembership.CurrentUserId, @group);
            }

            return graphGroup;
        }

        private TeamExtended createGraphTeam(UserEntryCollection users, Dictionary<string, TeamEntry> teamLookup, TeamEntry team)
        {
            if (teamLookup.ContainsKey(team.MailNickname))
            {
                throw new ArgumentException($"Trying to create 2 teams with same mail nickname ({team.MailNickname}). Only the first will be created.");
            }

            teamLookup.Add(team.MailNickname, team);
            var graphTeam = new TeamExtended(team.GroupId, team.Template);

            if (team?.Channels.Any() == true)
            {
                graphTeam.Channels = new Beta.TeamChannelsCollectionPage();
                // TODO: private channels are not supported currently. This will hopefully change :)
                foreach (var channel in team.Channels.Where(c => !c.IsPrivate))
                {
                    var graphChannel = createGraphChannel(users, channel);
                    graphTeam.Channels.Add(graphChannel);
                }
            }

            return graphTeam;
        }

        private Beta.Channel createGraphChannel(UserEntryCollection users, TeamChannelEntry channel)
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
                var channelMembers = new Beta.ChannelMembersCollectionPage();

                var owners = createChannelMemberCollection(users, channel.Owners, "owner");
                owners.ForEach(o => channelMembers.Add(o));

                var members = createChannelMemberCollection(users, channel.Members, "member");
                members.ForEach(o => channelMembers.Add(o));

                newChannel.Members = channelMembers;
            }

            return newChannel;
        }

        private Beta.Channel createPrivateGraphChannel(List<string> memberIds, List<string> ownerIds, string channelName)
        {
            var newChannel = new Beta.Channel()
            {
                DisplayName = channelName,
                MembershipType = Beta.ChannelMembershipType.Private
            };

            var channelMembers = new Beta.ChannelMembersCollectionPage();

            var owners = createChannelMemberCollection(ownerIds, "owner");
            owners.ForEach(o => channelMembers.Add(o));

            var members = createChannelMemberCollection(memberIds, "member");
            members.ForEach(o => channelMembers.Add(o));

            newChannel.Members = channelMembers;

            return newChannel;
        }

        private List<Beta.AadUserConversationMember> createChannelMemberCollection(List<string> userIds, string role)
        {
            var members = new List<Beta.AadUserConversationMember>();
            if (!userIds.Any())
            {
                return members;
            }

            foreach (var userId in userIds)
            {
                members.Add(new Beta.AadUserConversationMember
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"user@odata.bind", $"https://graph.microsoft.com/beta/users('{userId}')"}
                    },
                    Roles = new List<string>()
                    {
                        role
                    }
                });
            }

            return members;
        }
        private List<Beta.AadUserConversationMember> createChannelMemberCollection(UserEntryCollection users, IEnumerable<MemberEntry> memberEntries, string role)
        {
            var members = new List<Beta.AadUserConversationMember>();

            if (memberEntries == null)
            {
                // we do nothing
                return members;
            }

            foreach (var member in memberEntries)
            {
                var memberEntry = users.FindMember(member);
                if (memberEntry == null)
                {
                    // we want all or nothing
                    throw new MemberNotFoundException($"Channel {role} not found ({member.Name}).");
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

            return members;
        }

        /// <summary>
        /// If group has more than 20 owners/members combined we need to add them one by one (graph api problem)
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="users"></param>
        /// <returns>Returns groupIds of failed groups</returns>
        private async Task<HashSet<string>> addGroupMemberships(List<GroupEntry> groups, UserEntryCollection users)
        {
            if (!groups.Any())
            {
                return new HashSet<string>();
            }

            using var progressUpdater = new ProgressUpdater("Add Group Members/Owners", _notifier);
            var batchEntries = new List<GraphBatchRequest>();
            var failedGroupIds = new ConcurrentBag<string>();

            int i = 0;
            foreach (var group in groups)
            {
                var groupMembership = getGroupMemberships(users, @group, false);
                foreach (var ownerId in groupMembership.OwnerIds)
                {
                    batchEntries.Add(new GraphBatchRequest($"{group.GroupId}/{++i}", $"/groups/{group.GroupId}/owners/$ref", HttpMethod.Post, new GroupMember(ownerId)));
                }

                foreach (var memberId in groupMembership.MemberIds)
                {
                    batchEntries.Add(new GraphBatchRequest($"{group.GroupId}/{++i}", $"/groups/{group.GroupId}/members/$ref", HttpMethod.Post, new GroupMember(memberId)));
                }
            }

            progressUpdater.SetTotalCount(batchEntries.Count);
            await executeActionWithProgress(progressUpdater, batchEntries, onResult: (key, value) =>
            {
                if (!value.IsSuccessStatusCode)
                {
                    _notifier.Error($"Failed to add group owner/member. {getErrorMessage(value)}");
                    failedGroupIds.Add(key.Split('/')[0]);
                }
            });

            return new HashSet<string>(failedGroupIds.ToArray());
        }

        /// <summary>
        /// Maps owner/member ids
        /// </summary>
        /// <param name="users"></param>
        /// <param name="group"></param>
        /// <param name="insertCurrentUserToOwners">adds current user to owners if set to true</param>
        /// <returns></returns>
        private GroupMembershipDto getGroupMemberships(UserEntryCollection users, GroupEntry @group, bool insertCurrentUserToOwners = true)
        {
            bool userAddedToOwners = false;
            var ownerIds = users.GetMemberIds(@group.Owners);
            var memberIds = users.GetMemberIds(@group.Members);
            var currentUserUsername = _accessTokenManager.GetUsernameFromToken();
            var currentUserEntry = users.FindMember(new MemberEntry(currentUserUsername));
            if (insertCurrentUserToOwners && ownerIds.Any() && !ownerIds.Contains(currentUserEntry?.Id) && currentUserEntry != null)
            {
                ownerIds.Add(currentUserEntry.Id);
                userAddedToOwners = true;
            }

            return new GroupMembershipDto(ownerIds, memberIds, userAddedToOwners)
            {
                CurrentUserId = currentUserEntry?.Id
            };
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

            public bool HasTooManyMembers()
            {
                // graph api has a limit of 20 linked items in one request
                var ownerCount = OwnersODataBind?.Count() ?? 0;
                var memberCount = MembersODataBind?.Count() ?? 0;
                return (ownerCount + memberCount) >= 20;
            }
        }

        class TeamExtended : Beta.Team
        {
            [JsonProperty("group@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
            public string GroupBind { get; }

            [JsonProperty("template@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
            public string TemplateBind { get; }

            public TeamExtended(string groupId, string template)
            {
                GroupBind = $"https://graph.microsoft.com/v1.0/groups('{groupId}')";
                TemplateBind = $"https://graph.microsoft.com/beta/teamsTemplates('{template}')";
            }
        }

        class GroupMember
        {
            [JsonProperty("@odata.id", NullValueHandling = NullValueHandling.Ignore)]
            public string Id { get; }

            public GroupMember(string id)
            {
                Id = $"https://graph.microsoft.com/v1.0/directoryObjects/{id}";
            }
        }

        class GroupMembershipDto
        {
            public HashSet<string> OwnerIds { get; }
            public HashSet<string> MemberIds { get; }
            public bool CurrentUserAddedToOwners { get; }
            public string CurrentUserId { get; set; }

            public GroupMembershipDto(HashSet<string> ownerIds, HashSet<string> memberIds, bool currentUserAddedToOwners)
            {
                OwnerIds = ownerIds;
                MemberIds = memberIds;
                CurrentUserAddedToOwners = currentUserAddedToOwners;
            }
        }

        #endregion Helpers
    }
}

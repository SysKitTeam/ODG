using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
            var groupLookup = new Dictionary<string, UnifiedGroupEntry>();
            var successfullyCreatedGroups = new List<UnifiedGroupEntry>();
            var batchEntries = new List<GraphBatchRequest>();

            int i = 0;
            foreach (var group in groups)
            {
                if (tryCreateGroupBatch(users, groupLookup, @group, out var graphGroup))
                {
                    batchEntries.Add(new GraphBatchRequest(group.MailNickname, "groups", HttpMethod.Post, graphGroup));
                }
            }

            var tokenResult = await _accessTokenManager.GetGraphToken();
            var results = await _httpProvider.SendBatchAsync(batchEntries, tokenResult.Token);

            foreach (var result in results)
            {
                if (result.Value.IsSuccessStatusCode)
                {
                    var originalGroup = groupLookup[result.Key];
                    var createdGroup = _graphServiceClient.HttpProvider.Serializer.DeserializeObject<Group>(
                            await result.Value.Content.ReadAsStreamAsync());

                    originalGroup.GroupId = createdGroup.Id;
                    successfullyCreatedGroups.Add(originalGroup);
                }
                else
                {
                    _logger.Warning($"Failed to create group: {result.Key}. Status code: {(int)result.Value.StatusCode}");
                }
            }

            return successfullyCreatedGroups;
        }

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

        class GroupExtended : Group
        {
            [JsonProperty("owners@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
            public string[] OwnersODataBind { get; set; }
            [JsonProperty("members@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
            public string[] MembersODataBind { get; set; }
        }
    }
}

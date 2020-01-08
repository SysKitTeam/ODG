using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Graph;
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
                if (groupLookup.ContainsKey(group.MailNickname))
                {
                    _logger.Warning($"Trying to create 2 groups with same mail nickname ({group.MailNickname}). Only the first will be created.");
                }

                groupLookup.Add(group.MailNickname, group);
                var graphGroup = _autoMapper.Map<UnifiedGroupEntry, Group>(group, config => config.AfterMap((src, dest) =>
                    {
                        dest.Visibility = src.IsPrivate ? "Private" : "Public";
                        dest.MailEnabled = true;
                        dest.SecurityEnabled = false;
                        dest.GroupTypes = new List<string> { "Unified" };
                    }));

                batchEntries.Add(new GraphBatchRequest(group.MailNickname, "groups", HttpMethod.Post, graphGroup));
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
                    _logger.Warning($"Failed to create user: {result.Key}. Status code: {(int)result.Value.StatusCode}");
                }
            }

            return successfullyCreatedGroups;
        }
    }
}

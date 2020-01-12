using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupGenerationTask: IGenerationTask
    {
        private readonly IGroupDataGeneration _groupDataGeneration;
        private readonly IGraphApiClientFactory _graphApiClientFactory;
        private readonly ILogger _logger;

        public GroupGenerationTask(ILogger logger, IGroupDataGeneration groupDataGeneration, IGraphApiClientFactory graphApiClientFactory)
        {
            _logger = logger;
            _groupDataGeneration = groupDataGeneration;
            _graphApiClientFactory = graphApiClientFactory;
        }

        public async Task Execute(GenerationOptions options)
        {
            var userGraphApiClient = _graphApiClientFactory.CreateUserGraphApiClient(options.UserAccessTokenManager);
            var groupGraphApiClient = _graphApiClientFactory.CreateGroupGraphApiClient(options.UserAccessTokenManager);
            var users = await userGraphApiClient.GetAllTenantUsers(options.TenantDomain);

            var groups = _groupDataGeneration.CreateUnifiedGroupsAndTeams(options, users).ToList();

            if (groups.Any() == false)
            {
                return;
            }

            var createdGroups = await groupGraphApiClient.CreateUnifiedGroups(groups, users);
            var createdTeams = await groupGraphApiClient.CreateTeamsFromGroups(createdGroups.TeamsToCreate, users);
            await groupGraphApiClient.CreatePrivateChannels(createdTeams, users);
            // TODO: private channels
            // TODO: remove owners
        }
    }
}

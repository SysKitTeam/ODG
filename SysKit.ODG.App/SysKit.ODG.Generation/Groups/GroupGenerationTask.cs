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
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupGenerationTask: IGenerationTask
    {
        private readonly IGroupDataGeneration _groupDataGeneration;
        private readonly IGraphApiClientFactory _graphApiClientFactory;

        public GroupGenerationTask(IGroupDataGeneration groupDataGeneration, IGraphApiClientFactory graphApiClientFactory)
        {
            _groupDataGeneration = groupDataGeneration;
            _graphApiClientFactory = graphApiClientFactory;
        }

        public async Task Execute(GenerationOptions options, INotifier notifier)
        {
            var userGraphApiClient = _graphApiClientFactory.CreateUserGraphApiClient(options.UserAccessTokenManager, notifier);
            var groupGraphApiClient = _graphApiClientFactory.CreateGroupGraphApiClient(options.UserAccessTokenManager, notifier);
            var users = await userGraphApiClient.GetAllTenantUsers(options.TenantDomain);

            var groups = _groupDataGeneration.CreateUnifiedGroupsAndTeams(options, users).ToList();

            if (groups.Any() == false)
            {
                return;
            }

            var createdGroups = await groupGraphApiClient.CreateUnifiedGroups(groups, users);
            var createdTeams = await groupGraphApiClient.CreateTeamsFromGroups(createdGroups.TeamsToCreate, users);

            notifier.Info(new NotifyEntry("Group Generation", $"Created Office365 groups: {createdGroups.CreatedGroups.Count}; Created Teams: {createdTeams.Count}"));

            await groupGraphApiClient.CreateTeamChannels(createdTeams, users);

            // we needed to add ourselfs to owners so we can create teams
            var groupsToRemoveOwners = createdGroups.GroupsWithAddedOwners;
            if (groupsToRemoveOwners.Any())
            {
                // just in case, if there is some provisioning (public channels believed strangely(don't get created))
                await Task.Delay(TimeSpan.FromSeconds(10));
                await groupGraphApiClient.RemoveGroupOwners(createdGroups.GroupsWithAddedOwners);
            }
        }
    }
}

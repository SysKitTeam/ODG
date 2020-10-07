using System;
using System.Linq;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.DTO.Generation.Results;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupGenerationTask : IGenerationTask
    {
        private readonly IGroupDataGeneration _groupDataGeneration;
        private readonly IGraphApiClientFactory _graphApiClientFactory;
        private readonly ISharePointServiceFactory _sharePointServiceFactory;

        public GroupGenerationTask(IGroupDataGeneration groupDataGeneration, IGraphApiClientFactory graphApiClientFactory, ISharePointServiceFactory sharePointServiceFactory)
        {
            _groupDataGeneration = groupDataGeneration;
            _graphApiClientFactory = graphApiClientFactory;
            _sharePointServiceFactory = sharePointServiceFactory;
        }

        public async Task<IGenerationTaskResult> Execute(GenerationOptions options, INotifier notifier)
        {
            var userGraphApiClient = _graphApiClientFactory.CreateUserGraphApiClient(options.UserAccessTokenManager, notifier);
            var groupGraphApiClient = _graphApiClientFactory.CreateGroupGraphApiClient(options.UserAccessTokenManager, notifier);
            var users = await userGraphApiClient.GetAllTenantUsers(options.TenantDomain);
            var sharePointService = _sharePointServiceFactory.Create(options.UserCredentials, notifier);

            var groups = _groupDataGeneration.CreateUnifiedGroupsAndTeams(options, users).ToList();

            if (groups.Any() == false)
            {
                return null;
            }

            var createdGroups = await groupGraphApiClient.CreateUnifiedGroups(groups, users);
            var createdTeams = await groupGraphApiClient.CreateTeamsFromGroups(createdGroups.TeamsToCreate, users);
            var channelsCreated = await groupGraphApiClient.CreatePrivateTeamChannels(createdTeams.CreatedEntries, users);

            using (var progress = new ProgressUpdater("Populate Group Content", notifier))
            {
                progress.SetTotalCount(createdGroups.CreatedGroups.Count);
                foreach (var group in createdGroups.CreatedGroups)
                {
                    try
                    {
                        if (group.ProvisionFailed)
                        {
                            notifier.Error($"Failed to create content for {group.DisplayName} because group didn't provision");
                            continue;
                        }

                        group.SiteGuid = sharePointService.GetSiteCollectionGuid(group.SiteUrl);
                        await sharePointService.EnableAnonymousSharing(group.Url);
                        await sharePointService.SetMembershipOfDefaultSharePointGroups(group);
                        await sharePointService.CreateSharePointStructure(group);
                    }
                    catch (Exception ex)
                    {
                        notifier.Error($"Failed to create content for {group.DisplayName}", ex);
                        createdGroups.HasErrors = true;
                    }
                    finally
                    {
                        progress.UpdateProgress(1);
                    }
                }
            }

            // we needed to add ourselfs to owners so we can create teams
            var groupsToRemoveOwners = createdGroups.GroupsWithAddedOwners;
            var ownersRemovedOk = true;
            if (groupsToRemoveOwners.Any())
            {
                // just in case, if there is some provisioning (public channels behaved strangely(don't get created))
                await Task.Delay(TimeSpan.FromSeconds(10));
                ownersRemovedOk = await groupGraphApiClient.RemoveGroupOwners(createdGroups.GroupsWithAddedOwners);
            }

            notifier.Info($"Created Office365 groups: {createdGroups.CreatedGroups.Count}; Created Teams: {createdTeams.CreatedEntries.Count()}; Channels created ok: {channelsCreated}; Owners removed ok: {ownersRemovedOk}");

            // lts say channel errors are ok for now
            var hadErrors = createdGroups.HasErrors && createdTeams.HadErrors && !ownersRemovedOk;
            return new GroupGenerationTaskResult(createdGroups.CreatedGroups, hadErrors);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SysKit.ODG.Base;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.DTO.Generation.Results;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;
using SysKit.ODG.Base.Office365;
using SysKit.ODG.Base.Utils;

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

            var numberOfPrivateChannels = options.Template.RandomOptions.NumberOfPrivateChannels;
            var createPrivateChannels = numberOfPrivateChannels > 0;

            var createStructure = options.Template.RandomOptions.FillSitesWithContent;

            if (groups.Any() == false && createPrivateChannels == false && createStructure == false)
            {
                return null;
            }

            var totalGroupsCreated = 0;
            var totalTeamsCreated = 0;
            var totalOwnersRemovedOk = true;
            var allCreatedGroups = new List<UnifiedGroupEntry>();
            var hadGroupErrors = false;
            var hadTeamsErrors = false;

            var batchSize = 10;
            var page = 0;

            while (true)
            {
                var groupBatch = groups.Skip(batchSize * page).Take(batchSize).ToList();
                page++;
                if (!groupBatch.Any())
                {
                    break;
                }
                var createdGroups = await groupGraphApiClient.CreateUnifiedGroups(groupBatch, users);
                totalGroupsCreated += createdGroups.CreatedGroups.Count;
                allCreatedGroups.AddRange(createdGroups.CreatedGroups);
                hadGroupErrors = hadGroupErrors || createdGroups.HasErrors;

                var createdTeams = await groupGraphApiClient.CreateTeamsFromGroups(createdGroups.TeamsToCreate, users);
                totalTeamsCreated += createdTeams.CreatedEntries.Count();
                hadTeamsErrors = hadGroupErrors || createdTeams.HadErrors;

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

                            group.SiteGuid = await sharePointService.GetSiteCollectionGuid(group.SiteUrl);
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
                totalOwnersRemovedOk = totalOwnersRemovedOk && ownersRemovedOk;
                notifier.Info($"Batch: Created Office365 groups: {createdGroups.CreatedGroups.Count}; Created Teams: {createdTeams.CreatedEntries.Count()}; Owners removed ok: {ownersRemovedOk}");
                notifier.Info($"Total: Created Office365 groups: {totalGroupsCreated}; Created Teams: {totalTeamsCreated}; Owners removed ok: {totalOwnersRemovedOk}");
            }

            if (createPrivateChannels)
            {
                var teamIds = await groupGraphApiClient.GetAllTenantTeamIds();
                var teamsForPrivateChannels = teamIds.GetRandom(numberOfPrivateChannels);
                var membershipLookup = await groupGraphApiClient.GetTeamMembers(teamsForPrivateChannels.ToList());
                var privateChannelsToCreate = _groupDataGeneration.CreatePrivateChannels(membershipLookup);
                var createdChannels = await groupGraphApiClient.CreatePrivateTeamChannels(privateChannelsToCreate);
                await Task.Delay(TimeSpan.FromSeconds(15));
                await groupGraphApiClient.ProvisionPrivateChannelSites(createdChannels);
            }

            if (createStructure)
            {
                await createSiteStructures(sharePointService, groupGraphApiClient, users, notifier, options);
            }

            // lts say channel errors are ok for now
            var hadErrors = hadGroupErrors && hadTeamsErrors && !totalOwnersRemovedOk;
            return new GroupGenerationTaskResult(allCreatedGroups, hadErrors);
        }

        private const int IsContentGeneratedThreshold = 500;

        private async Task createSiteStructures(ISharePointService sharePointService, IGroupGraphApiClient graphApiClient, UserEntryCollection users, INotifier notifier, GenerationOptions options)
        {
            var siteUrls = await sharePointService.GetAllSiteCollectionUrls();
            var groupEmails = await graphApiClient.GetAllTenantGroupEmails();
            using (var progress = new ProgressUpdater("Populate Site Content", notifier))
            {
                progress.SetTotalCount(siteUrls.Count);

                foreach (var siteUrl in siteUrls)
                {
                    try
                    {

                        var siteEntry = new SiteEntry()
                        {
                            Owner = new MemberEntry("mock.owner"),
                            Url = siteUrl
                        };

                        using (_sharePointServiceFactory.CreateElevatedScope(options.UserCredentials, siteEntry))
                        {
                            var isDefaultDocumentLibraryFilledWithData =
                            await sharePointService.IsDefaultDocumentLibraryFilledWithData(
                                siteUrl,
                                IsContentGeneratedThreshold);
                            if (isDefaultDocumentLibraryFilledWithData)
                            {
                                notifier.Warning($"Skipped site {siteUrl}. It is filled with data.");
                                continue;
                            }

                            var itemsPerSite = RandomThreadSafeGenerator.Next(1000, 1100);
                            var structure = _groupDataGeneration.GenerateDocumentsFolderStructure(itemsPerSite, users, groupEmails);
                            notifier.Info($"Generating content for {siteUrl}");
                            sharePointService.CreateSharePointFolderStructure(siteUrl, structure, notifier);
                        }
                    }
                    catch (Exception ex)
                    {
                        notifier.Error($"Failed to create content for {siteUrl}", ex);
                    }
                    finally
                    {
                        progress.UpdateProgress(1);
                    }
                }
            }
        }
    }
}

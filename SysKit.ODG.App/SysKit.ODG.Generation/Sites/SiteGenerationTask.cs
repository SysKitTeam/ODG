using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.DTO.Generation.Results;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Generation.Sites
{
    public class SiteGenerationTask : IGenerationTask
    {
        private readonly ISiteDataGeneration _siteDataGeneration;
        private readonly ISharePointServiceFactory _sharePointServiceFactory;

        public SiteGenerationTask(ISiteDataGeneration siteDataGeneration, ISharePointServiceFactory sharePointServiceFactory)
        {
            _siteDataGeneration = siteDataGeneration;
            _sharePointServiceFactory = sharePointServiceFactory;
        }

        public async Task<IGenerationTaskResult> Execute(GenerationOptions options, INotifier notifier)
        {
            var createdSites = new List<SiteEntry>();
            var sites = _siteDataGeneration.CreateSites(options).ToList();
            var sharePointService = _sharePointServiceFactory.Create(options.UserCredentials, notifier);

            if (sites.Any() == false)
            {
                return null;
            }

            using (var progress = new ProgressUpdater("Create Sites", notifier))
            {
                progress.SetTotalCount(sites.Count);
                foreach (var site in sites)
                {
                    try
                    {
                        await sharePointService.CreateSite(site);

                        using (_sharePointServiceFactory.CreateElevatedScope(options.UserCredentials, site))
                        {
                            await sharePointService.SetMembershipOfDefaultSharePointGroups(site);
                            await sharePointService.CreateSharePointStructure(site);
                        }

                        createdSites.Add(site);
                    }
                    catch (Exception ex)
                    {
                        notifier.Error($"Failed to create {site.Title}", ex);
                    }
                    finally
                    {
                        progress.UpdateProgress(1);
                    }
                }
            }
            

            return new SiteGenerationTaskResult(createdSites, createdSites.Count != sites.Count);
        }
    }
}

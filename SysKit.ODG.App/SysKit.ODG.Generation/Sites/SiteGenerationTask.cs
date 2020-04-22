using System;
using System.Linq;
using System.Threading.Tasks;
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
            var sites = _siteDataGeneration.CreateSites(options).ToList();
            var sharePointService = _sharePointServiceFactory.Create(options.UserCredentials);

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
                        progress.UpdateProgress(1);
                        //await sharePointService.CreateSharePointStructure(site.Url);
                    }
                    catch (Exception ex)
                    {
                        notifier.Error($"Failed to create {site.Title}", ex);
                    }

                }
            }
            

            // TODO: return created sites
            return new SiteGenerationTaskResult();
        }
    }
}

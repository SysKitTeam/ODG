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
        private readonly ISharePointService _sharePointService;

        public SiteGenerationTask(ISiteDataGeneration siteDataGeneration, ISharePointService sharePointService)
        {
            _siteDataGeneration = siteDataGeneration;
            _sharePointService = sharePointService;
        }

        public async Task<IGenerationTaskResult> Execute(GenerationOptions options, INotifier notifier)
        {
            var sites = _siteDataGeneration.CreateSites(options).ToList();

            if (sites.Any() == false)
            {
                return null;
            }

            foreach (var site in sites)
            {
                await _sharePointService.CreateSite(options.UserAccessTokenManager, site);
            }

            // TODO: return created sites
            return new SiteGenerationTaskResult();
        }
    }
}

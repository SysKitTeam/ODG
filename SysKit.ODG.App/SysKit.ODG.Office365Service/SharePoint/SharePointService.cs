using System;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Sites;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;

namespace SysKit.ODG.Office365Service.SharePoint
{
    public class SharePointService : ISharePointService
    {
        public SharePointService()
        {

        }

        public async Task<bool> CreateSite(IAccessTokenManager accessTokenManager, SiteEntry site)
        {
            try
            {
                var rootContext = await getClientContext(getTenantUrl(site.Url), accessTokenManager);

                var newSite = await SiteCollection.CreateAsync(rootContext, new CommunicationSiteCollectionCreationInformation
                {
                    Title = site.Title,
                    Url = site.Url,
                    Owner = accessTokenManager.GetUsernameFromToken()
                });

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected async Task<ClientContext> getClientContext(string siteUrl, IAccessTokenManager accessTokenManager)
        {
            var manager = new AuthenticationManager();
            return manager.GetAzureADAccessTokenAuthenticatedContext(siteUrl, (await accessTokenManager.GetSharePointToken()).Token);
        }

        private string getTenantUrl(string siteUrl)
        {
            var host = new Uri(siteUrl);
            return $"{host.Scheme}://{host.Host}";
        }
    }
}

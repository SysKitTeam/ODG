using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Entities;
using OfficeDevPnP.Core.Sites;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.Interfaces.Office365Service;

namespace SysKit.ODG.Office365Service.SharePoint
{
    internal class SharePointService : ISharePointService
    {
        private readonly SimpleUserCredentials _userCredentials;
        public SharePointService(SimpleUserCredentials userCredentials)
        {
            _userCredentials = userCredentials;
        }

        public async Task CreateSite(SiteEntry site)
        {
            using (var rootContext = getClientContext(getAdminUrl(site.Url)))
            {
                Tenant tenant = new Tenant(rootContext);
                if (tenant.SiteExistsAnywhere(site.Url) != SiteExistence.No)
                {
                    throw new SiteAlreadyExists(site.Url);
                }

                var siteInfo = new CommunicationSiteCollectionCreationInformation
                {
                    Title = site.Title,
                    Url = site.Url,
                    // so we can set other admins
                    Owner = _userCredentials.Username
                };

                var newSite = await SiteCollection.CreateAsync(rootContext, siteInfo, 15);

                if (site.SiteAdmins?.Any() == true)
                {
                    newSite.Web.AddAdministrators(site.SiteAdmins.Select(admin => new UserEntity{ LoginName = getLoginNameFromEntry(admin, site.Url)} ).ToList());
                }

                if (site.Owner != null)
                {
                    var ownerName = getLoginNameFromEntry(site.Owner, site.Url);
                    if (!ownerName.Equals(_userCredentials.Username, StringComparison.OrdinalIgnoreCase))
                    {
                        // set real SC owner
                        newSite.Web.EnsureUser(ownerName);

                    }
                }
            }
        }

        public async Task CreateSharePointStructure(string url)
        {
            using (var context = getClientContext(url))
            {
                var web = context.Web;
                context.Load(web, w => w.CurrentUser.IsSiteAdmin);
                //context.Load(web);
                await context.ExecuteQueryRetryAsync();

                var isUserAdmin = context.Web.CurrentUser.IsSiteAdmin;

                var docLib = web.DefaultDocumentLibrary().RootFolder;
                context.Load(docLib, l => l.ServerRelativeUrl);
                await context.ExecuteQueryRetryAsync();
                var newFolder = web.EnsureFolder(docLib, "TestFolder");
            }
        }

        protected ClientContext getClientContext(string siteUrl)
        {
            var manager = new AuthenticationManager();
            return manager.GetSharePointOnlineAuthenticatedContextTenant(siteUrl, _userCredentials.Username, _userCredentials.Password);
        }

        private string getLoginNameFromEntry(MemberEntry memberEntry, string siteUrl)
        {
            if (memberEntry.IsFQDN)
            {
                return memberEntry.Name;
            }

            var host = new Uri(siteUrl);
            return $"{memberEntry.Name}@{host.Host.Replace(".sharepoint.com", ".onmicrosoft.com")}";
        }

        private string getTenantUrl(string siteUrl)
        {
            var host = new Uri(siteUrl);
            return $"{host.Scheme}://{host.Host}";
        }

        private string getAdminUrl(string siteUrl)
        {
            var host = new Uri(siteUrl);
            return $"{host.Scheme}://{host.Host.Replace(".sharepoint.com", "-admin.sharepoint.com")}";
        }
    }
}

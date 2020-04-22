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
                    // so I can do all other actions, will remove at the end
                    Owner = _userCredentials.Username
                };

                var newSite = await SiteCollection.CreateAsync(rootContext, siteInfo, 15);

                if (site.SiteAdmins?.Any() == true)
                {
                    newSite.Web.AddAdministrators(site.SiteAdmins.Select(admin => new UserEntity{ LoginName = getLoginNameFromEntry(admin, site.Url)} ).ToList());
                }
            }
        }

        public async Task SetSiteOwner(SiteEntry site)
        {
            var realOwner = getLoginNameFromEntry(site.Owner, site.Url);

            if (!_userCredentials.Username.Equals(realOwner, StringComparison.OrdinalIgnoreCase))
            {
                using (var context = getClientContext(site.Url))
                {
                    var realOwnerUser = context.Web.EnsureUser(realOwner);
                    context.Site.Owner = realOwnerUser;
                    context.Load(context.Site.Owner);
                    await context.ExecuteQueryRetryAsync();
                }
            }

            if (site.SiteAdmins?.Any() != true)
            {
                return;
            }

            // check if _userCredentials.Username is in site admins, because Owner change just removed him
            if (site.SiteAdmins.All(admin =>
                !_userCredentials.Username.Equals(getLoginNameFromEntry(admin, site.Url),
                    StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            using (var rootContext = getClientContext(getAdminUrl(site.Url)))
            {
                Tenant tenant = new Tenant(rootContext);
                tenant.SetSiteAdmin(site.Url, _userCredentials.Username, true);
                await tenant.Context.ExecuteQueryRetryAsync();
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

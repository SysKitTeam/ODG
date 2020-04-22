using System;
using System.Collections.Generic;
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
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Office365Service.SharePoint
{
    internal class SharePointService : ISharePointService
    {
        private readonly SimpleUserCredentials _userCredentials;
        private readonly INotifier _notifier;

        public SharePointService(SimpleUserCredentials userCredentials, INotifier notifier)
        {
            _userCredentials = userCredentials;
            _notifier = notifier;
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
            //// Clean _userCredentials.Username from SharePoint owners group (it is forced into it, don't know why)
            //if (site.SPOwners?.Any(o => _userCredentials.Username.Equals(getLoginNameFromEntry(o, site.Url))) != true)
            //{
            //    using (var context = getClientContext(site.Url))
            //    {
            //        var ownersGroup = context.Web.AssociatedOwnerGroup;
            //        context.Load(ownersGroup, g => g.LoginName);
            //        await context.ExecuteQueryRetryAsync();
            //        context.Web.RemoveUserFromGroup(ownersGroup.LoginName, _userCredentials.Username);
            //    }
            //}

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
                tenant.AddAdministrators(new List<UserEntity> { new UserEntity { LoginName = _userCredentials.Username } }, new Uri(site.Url), false);
                await tenant.Context.ExecuteQueryRetryAsync();
            }
        }

        public async Task SetMembershipOfDefaultSharePointGroups(SiteEntry site)
        {
            using (var context = getClientContext(site.Url))
            {
                Task clearExistingUsers(Group group)
                {
                    //group.EnsureProperties(g => g.Users);
                    while (group.Users.Count > 0)
                    {
                        var user = group.Users[0];
                        group.Users.Remove(user);
                    }
                    group.Update();
                    return group.Context.ExecuteQueryRetryAsync();
                }

                async Task addGroupMembers(List<MemberEntry> members, Group group)
                {
                    if (group == null)
                    {
                        return;
                    }

                    await clearExistingUsers(group);

                    if (members?.Any() != true)
                    {
                        return;
                    }

                    foreach (var member in members)
                    {
                        try
                        {
                            context.Web.AddUserToGroup(group, getLoginNameFromEntry(member, site.Url));
                        }
                        catch(Exception ex)
                        {
                            // user not found exception, for now only log it
                            _notifier.Warning($"Failed to add {member.Name} to group. Error: {ex.Message}");
                        }
                    }
                }

                context.Load(context.Web, x => x.AssociatedMemberGroup.Users);
                context.Load(context.Web, x => x.AssociatedOwnerGroup.Users);
                context.Load(context.Web, x => x.AssociatedVisitorGroup.Users);
                await context.ExecuteQueryRetryAsync();

                await addGroupMembers(site.SPMembers, context.Web.AssociatedMemberGroup);
                await addGroupMembers(site.SPOwners, context.Web.AssociatedOwnerGroup);
                await addGroupMembers(site.SPVisitors, context.Web.AssociatedVisitorGroup);
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

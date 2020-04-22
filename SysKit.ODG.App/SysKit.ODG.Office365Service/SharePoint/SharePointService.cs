using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
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
            using (var rootContext = SharePointUtils.CreateAdminContext(_userCredentials))
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
                    Owner = SharePointUtils.GetLoginNameFromEntry(site.Owner, site.Url)
                };

                var newSite = await SiteCollection.CreateAsync(rootContext, siteInfo, 15);

                using (new ElevatedSharePointScope(site, _userCredentials))
                {
                    if (site.SiteAdmins?.Any() == true)
                    {
                        newSite.Web.AddAdministrators(site.SiteAdmins.Select(admin => new UserEntity { LoginName = SharePointUtils.GetLoginNameFromEntry(admin, site.Url) }).ToList());
                    }
                }
            }
        }

        public async Task SetMembershipOfDefaultSharePointGroups(SiteEntry site)
        {
            using (var context = SharePointUtils.CreateClientContext(site.Url, _userCredentials))
            {
                Task clearExistingUsers(Group group)
                {
                    group.EnsureProperties(g => g.Users);
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
                            context.Web.AddUserToGroup(group, SharePointUtils.GetLoginNameFromEntry(member, site.Url));
                        }
                        catch (Exception ex)
                        {
                            // user not found exception, for now only log it
                            _notifier.Warning($"Failed to add {member.Name} to group. Error: {ex.Message}");
                        }
                    }
                }

                context.Load(context.Web, x => x.AssociatedOwnerGroup.Users);
                context.Load(context.Web, x => x.AssociatedMemberGroup.Users);
                context.Load(context.Web, x => x.AssociatedVisitorGroup.Users);
                await context.ExecuteQueryRetryAsync();

                await addGroupMembers(site.SPOwners, context.Web.AssociatedOwnerGroup);
                await addGroupMembers(site.SPMembers, context.Web.AssociatedMemberGroup);
                await addGroupMembers(site.SPVisitors, context.Web.AssociatedVisitorGroup);
            }
        }


        public async Task CreateSharePointStructure(string url)
        {
            using (var context = SharePointUtils.CreateClientContext(url, _userCredentials))
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
    }
}

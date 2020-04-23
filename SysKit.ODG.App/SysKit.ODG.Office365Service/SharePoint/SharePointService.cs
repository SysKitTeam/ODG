﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core.Entities;
using OfficeDevPnP.Core.Sites;
using OfficeDevPnP.Core.Utilities;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Enums;
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

        #region SharePoint Structure and Permissions

        public async Task CreateSharePointStructure(ISharePointContent sharePointContent)
        {
            if (sharePointContent?.Content == null)
            {
                return;
            }

            using (var context = SharePointUtils.CreateClientContext(sharePointContent.Url, _userCredentials))
            {
                var rootWeb = context.Site.RootWeb;

                // handle permissions for root web
                assignPermissions(rootWeb, sharePointContent.Content, true);

                foreach (var content in sharePointContent.Content.Children)
                {
                    switch (content.Type)
                    {
                        case ContentTypeEnum.Web:
                            createSubsite(rootWeb, content, context);
                            break;
                        case ContentTypeEnum.DocumentLibrary:
                            createDocumentLibrary(rootWeb, content, context);
                            break;
                    }
                }
            }
        }

        private void createSubsite(Web parentWeb, ContentEntry webContent, ClientContext context)
        {
            // web exists
            // get web on Web object
            Web web = parentWeb.CreateWeb(new SiteEntity
            {
                Title = webContent.Name,
                Url = UrlUtility.StripInvalidUrlChars(webContent.Name)
            });

            // handle web permissions
            assignPermissions(web, webContent);

            foreach (var content in webContent.Children)
            {
                switch (content.Type)
                {
                    case ContentTypeEnum.Web:
                        createSubsite(web, content, context);
                        break;
                    case ContentTypeEnum.DocumentLibrary:
                        createDocumentLibrary(web, content, context);
                        break;
                }
            }
        }

        private void createDocumentLibrary(Web parentWeb, ContentEntry listContent, ClientContext context)
        {
           var list = parentWeb.CreateDocumentLibrary(listContent.Name);
           
            // handle list permissions
            assignPermissions(list, listContent);

            foreach (var content in listContent.Children)
            {
                switch (content.Type)
                {
                    case ContentTypeEnum.Folder:
                        createFolder(parentWeb, list.RootFolder, content, context);
                        break;
                    case ContentTypeEnum.File:
                        createFile(parentWeb, list.RootFolder, content, context);
                        break;
                }
            }
        }

        private void createFolder(Web parentWeb, Folder parentFolder, ContentEntry folderContent, ClientContext context)
        {
            var folder = parentFolder.CreateFolder(folderContent.Name);
            var test = folder.UniqueId;

            assignPermissions(folder.ListItemAllFields, folderContent);

            foreach (var content in folderContent.Children)
            {
                switch (content.Type)
                {
                    case ContentTypeEnum.Folder:
                        createFolder(parentWeb, folder, content, context);
                        break;
                    case ContentTypeEnum.File:
                        createFile(parentWeb, folder, content, context);
                        break;
                }
            }
        }

        private void createFile(Web parentWeb, Folder parentFolder, ContentEntry fileContent, ClientContext context)
        {
            // create file
            //throw new NotImplementedException();
            return;
        }

        private void assignPermissions(SecurableObject secObject, IRoleAssignments secInfo, bool isRootWeb = false)
        {
            if (!isRootWeb && !secInfo.HasUniqueRoleAssignments)
            {
                return;
            }

            if (!isRootWeb)
            {
                secObject.BreakRoleInheritance(secInfo.CopyFromParent, false);
                secObject.Context.ExecuteQueryRetry();
            }

            foreach (var roleAssignment in secInfo.Assignments)
            {
                var role = getRoleType(roleAssignment.Key);
                foreach (var userAss in roleAssignment.Value)
                {
                    secObject.AddPermissionLevelToUser(SharePointUtils.GetLoginNameFromEntry(userAss, secObject.Context.Url), role);
                }
            }
        }

        private RoleType getRoleType(RoleTypeEnum odgRole)
        {
            switch (odgRole)
            {
                case RoleTypeEnum.FullControl:
                    return RoleType.Administrator;
                case RoleTypeEnum.Read:
                    return RoleType.Reader;
                case RoleTypeEnum.Contributor:
                    return RoleType.Contributor;
                default:
                    return RoleType.None;
            }
        }

        #endregion SharePoint Structure and Permissions

    }
}

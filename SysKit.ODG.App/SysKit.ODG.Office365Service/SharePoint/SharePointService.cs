using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.Online.SharePoint.TenantManagement;
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

        /// <inheritdoc />
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
                tenant.SetSiteProperties(siteInfo.Url,
                    sharingCapability: SharingCapabilities.ExternalUserAndGuestSharing);

                using (new ElevatedSharePointScope(site, _userCredentials))
                {
                    if (site.SiteAdmins?.Any() == true)
                    {
                        newSite.Web.AddAdministrators(site.SiteAdmins.Select(admin => new UserEntity
                            {LoginName = SharePointUtils.GetLoginNameFromEntry(admin, site.Url)}).ToList());
                    }
                }
            }
        }

        /// <inheritdoc />
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

                    //await clearExistingUsers(group);

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

        /// <inheritdoc />
        public async Task EnableAnonymousSharing(string url)
        {
            using (var rootContext = SharePointUtils.CreateAdminContext(_userCredentials))
            {
                Tenant tenant = new Tenant(rootContext);
                tenant.SetSiteProperties(url, sharingCapability: SharingCapabilities.ExternalUserAndGuestSharing);
            }
        }

        /// <inheritdoc />
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
                        createFolder(parentWeb, list, list.RootFolder, content, context);
                        break;
                    case ContentTypeEnum.File:
                        createFile(parentWeb, list, list.RootFolder, content, context);
                        break;
                }
            }
        }

        private void createFolder(Web parentWeb, List parentList, Folder parentFolder, ContentEntry folderContent,
            ClientContext context)
        {
            var folder = parentFolder.CreateFolder(folderContent.Name);

            assignPermissions(folder.ListItemAllFields, folderContent);
            assignSharingLinks(parentWeb, folder.ServerRelativeUrl, folderContent);

            foreach (var content in folderContent.Children)
            {
                switch (content.Type)
                {
                    case ContentTypeEnum.Folder:
                        createFolder(parentWeb, parentList, folder, content, context);
                        break;
                    case ContentTypeEnum.File:
                        createFile(parentWeb, parentList, folder, content, context);
                        break;
                }
            }
        }

        private void createFile(Web parentWeb, List parentList, Folder parentFolder, ContentEntry fileContent,
            ClientContext context)
        {
            Microsoft.SharePoint.Client.File newFile;
            UnicodeEncoding uniEncoding = new UnicodeEncoding();
            String message = "Random file message";

            using (MemoryStream ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms, uniEncoding);
                try
                {
                    sw.Write(message);
                    sw.Flush(); //otherwise you are risking empty stream
                    ms.Seek(0, SeekOrigin.Begin);

                    newFile = parentFolder.UploadFile(fileContent.Name, ms, false);
                }
                finally
                {
                    sw.Dispose();
                }
            }

            assignPermissions(newFile.ListItemAllFields, fileContent);
            assignSharingLinks(parentWeb, newFile.ServerRelativeUrl, fileContent);
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

            // for some reason owner of the document is automatically added if we don't copy permissions from parent
            if (!isRootWeb && !secInfo.CopyFromParent)
            {
                secObject.RemovePermissionLevelFromUser(_userCredentials.Username,
                    getRoleType(RoleTypeEnum.FullControl), true);
            }

            foreach (var roleAssignment in secInfo.Assignments)
            {
                var role = getRoleType(roleAssignment.Key);
                foreach (var userAss in roleAssignment.Value)
                {
                    secObject.AddPermissionLevelToUser(
                        SharePointUtils.GetLoginNameFromEntry(userAss, secObject.Context.Url), role);
                }
            }
        }

        private void assignSharingLinks(Web parentWeb, string itemServerRelativeUrl, IRoleAssignments secInfo)
        {
            if (!secInfo.HasUniqueRoleAssignments)
            {
                return;
            }

            var siteUri = new Uri(parentWeb.Context.Url);
            var fullItemUrl = $"https://{siteUri.Host}{itemServerRelativeUrl}";
            foreach (var sharingLink in secInfo.SharingLinks)
            {
                parentWeb.CreateAnonymousLinkForDocument(fullItemUrl,
                    sharingLink.IsEdit ? ExternalSharingDocumentOption.Edit : ExternalSharingDocumentOption.View);
                // you need to click on this link to be visible so for now we don't try to create them
                // parentWeb.ShareDocument(fullItemUrl, _userCredentials.Username, ExternalSharingDocumentOption.View);
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

        /// <inheritdoc />
        public bool DeleteSiteCollection(string siteUrl)
        {
            using (var rootContext = SharePointUtils.CreateAdminContext(_userCredentials))
            {
                Tenant tenant = new Tenant(rootContext);

                bool deleteSite(int attempt = 1)
                {
                    if (attempt >= 5)
                    {
                        return false;
                    }

                    if (attempt > 1)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(attempt * 10));
                    }

                    try
                    {
                        var siteDeleted = tenant.DeleteSiteCollection(siteUrl, false);
                        if (!siteDeleted)
                        {
                            return deleteSite(attempt - 1);
                        }

                        return true;
                    }
                    catch (Exception e)
                    {
                        _notifier.Error($"Error while deleting site: {siteUrl}", e);
                        return deleteSite(attempt - 1);
                    }
                }

                return deleteSite();
            }
        }
    }
}

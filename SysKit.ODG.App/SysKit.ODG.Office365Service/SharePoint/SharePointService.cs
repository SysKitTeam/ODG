using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using SysKit.ODG.Common.DTO.Generation;
using SharingLinkType = SysKit.ODG.Common.DTO.Generation.SharingLinkType;

namespace SysKit.ODG.Office365Service.SharePoint
{
    internal class SharePointService : ISharePointService
    {
        private readonly SimpleUserCredentials _userCredentials;
        private readonly INotifier _notifier;
        private readonly ISharePointFileService _fileService;

        public SharePointService(SimpleUserCredentials userCredentials, INotifier notifier, ISharePointFileService fileService)
        {
            _userCredentials = userCredentials;
            _notifier = notifier;
            _fileService = fileService;
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
                    newSite.Load(newSite.Site, s => s.Id);
                    await newSite.ExecuteQueryAsync();
                    site.SiteGuid = newSite.Site.Id;
                    if (site.SiteAdmins?.Any() == true)
                    {
                        newSite.Web.AddAdministrators(site.SiteAdmins.Select(admin => new UserEntity
                        { LoginName = SharePointUtils.GetLoginNameFromEntry(admin, site.Url) }).ToList());
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task SetMembershipOfDefaultSharePointGroups(IAssociatedSPGroups site)
        {
            if (site.SPMembers?.Any() == false && site.SPOwners?.Any() == false && site.SPVisitors?.Any() == false)
            {
                return;
            }

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

        /// <inheritdoc />
        public void CreateSharePointFolderStructure(string url, List<ContentEntry> contentOfRootFolder)
        {
            if (contentOfRootFolder == null || contentOfRootFolder.Count == 0)
            {
                return;
            }

            using (var context = SharePointUtils.CreateClientContext(url, _userCredentials))
            {
                var rootWeb = context.Site.RootWeb;
                var documentLibrary = context.Site.RootWeb.DefaultDocumentLibrary();
                foreach (var content in contentOfRootFolder)
                {
                    switch (content.Type)
                    {
                        case ContentTypeEnum.Folder:
                            createFolder(rootWeb, documentLibrary, documentLibrary.RootFolder, content, context);
                            break;
                        case ContentTypeEnum.File:
                            createFile(rootWeb, documentLibrary, documentLibrary.RootFolder, content, context);
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

            var extension = ".txt";
            var name = fileContent.Name;
            if (fileContent is FileEntry fileEntry)
            {
                extension = fileEntry.Extension;
                name = fileEntry.NameWithExtension;
            }

            using (var ms = getStreamForExtension(extension))
            {
                newFile = parentFolder.UploadFile(name, ms, false);
            }

            assignPermissions(newFile.ListItemAllFields, fileContent);
            assignSharingLinks(parentWeb, newFile.ServerRelativeUrl, fileContent);
        }

        private Stream getStreamForExtension(string extension)
        {
            string[] resourceNames =
                Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var fileName = _fileService.GetExtensionFileNamesLookup().TryGetValue(extension, out var fileNameOut)
                ? fileNameOut
                : _fileService.GetExtensionFileNamesLookup()[".txt"];
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SysKit.ODG.Office365Service.SharePoint.FileTemplates.{fileName}";

            return assembly.GetManifestResourceStream(resourceName);
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
                switch (sharingLink.SharingLinkType)
                {
                    case SharingLinkType.Anonymous:
                        parentWeb.CreateAnonymousLinkForDocument(fullItemUrl,
                            sharingLink.IsEdit ? ExternalSharingDocumentOption.Edit : ExternalSharingDocumentOption.View);
                        break;
                    case SharingLinkType.Company:
                        Web.CreateOrganizationSharingLink(parentWeb.Context, fullItemUrl, sharingLink.IsEdit);
                        break;
                    case SharingLinkType.Specific:
                        var userEmail = _userCredentials.Username;
                        if (sharingLink is SpecificSharingLinkEntry specificLink)
                        {
                            userEmail = specificLink.SharedWithEmail;
                        }
                        parentWeb.ShareDocument(fullItemUrl, userEmail, sharingLink.IsEdit ? ExternalSharingDocumentOption.Edit : ExternalSharingDocumentOption.View);
                        break;
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
                            return deleteSite(attempt + 1);
                        }

                        return true;
                    }
                    catch (Exception e)
                    {
                        _notifier.Error($"Error while deleting site: {siteUrl}", e);
                        return deleteSite(attempt + 1);
                    }
                }

                return deleteSite();
            }
        }

        /// <inheritdoc />
        public async Task<Guid> GetSiteCollectionGuid(string siteUrl)
        {
            using (var rootContext = SharePointUtils.CreateAdminContext(_userCredentials))
            {
                Tenant tenant = new Tenant(rootContext);
                var site = tenant.GetSiteByUrl(siteUrl);
                rootContext.Load(site, s => s.Id);
                await rootContext.ExecuteQueryAsync();
                return site.Id;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface ISharePointService
    {
        /// <summary>
        /// Creates new site or throws an error if it already exists
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task CreateSite(SiteEntry site);

        /// <summary>
        /// Creates SharePoint content and assignees permissions
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        Task CreateSharePointStructure(ISharePointContent content);

        /// <summary>
        /// Creates a folder structure in default document library
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contentOfRootFolder"></param>
        void CreateSharePointFolderStructure(string url, List<ContentEntry> contentOfRootFolder);

        /// <summary>
        /// Sets membership for Owners, Visitors and Members group
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task SetMembershipOfDefaultSharePointGroups(IAssociatedSPGroups site);

        /// <summary>
        /// Enable anonymous sharing so we can create sharing links
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task EnableAnonymousSharing(string url);

        /// <summary>
        /// Completely deletes site
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        bool DeleteSiteCollection(string siteUrl);

        /// <summary>
        /// Returns SharePoint Id for site url
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        Task<Guid> GetSiteCollectionGuid(string siteUrl);

        /// <summary>
        /// Returns a list of file extensions that can be created
        /// </summary>
        /// <returns></returns>
        List<string> GetFileExtensions();
    }
}

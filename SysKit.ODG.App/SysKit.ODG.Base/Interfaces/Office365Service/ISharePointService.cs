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
        /// Sets membership for Owners, Visitors and Members group
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Task SetMembershipOfDefaultSharePointGroups(SiteEntry site);

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
    }
}

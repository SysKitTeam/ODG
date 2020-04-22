using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface ISharePointService
    {
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
    }
}

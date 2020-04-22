using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface ISharePointService
    {
        Task CreateSite(SiteEntry site);

        Task CreateSharePointStructure(string url);

        Task SetSiteOwner(SiteEntry site);
    }
}

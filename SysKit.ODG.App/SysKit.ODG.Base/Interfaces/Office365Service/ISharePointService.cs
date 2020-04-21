using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface ISharePointService
    {
        Task<bool> CreateSite(IAccessTokenManager accessTokenManager, SiteEntry site);
    }
}

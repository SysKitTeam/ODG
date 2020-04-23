using System.Threading.Tasks;
using SysKit.ODG.Base.Authentication;

namespace SysKit.ODG.Base.Interfaces.Authentication
{
    public interface IAccessTokenManager
    {
        string GetUsernameFromToken();
        Task<AuthToken> GetGraphToken();
    }
}

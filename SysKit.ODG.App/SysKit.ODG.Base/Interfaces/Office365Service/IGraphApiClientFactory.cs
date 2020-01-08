using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface IGraphApiClientFactory
    {
        IUserGraphApiClient CreateUserGraphApiClient(IAccessTokenManager accessTokenManager);
    }
}
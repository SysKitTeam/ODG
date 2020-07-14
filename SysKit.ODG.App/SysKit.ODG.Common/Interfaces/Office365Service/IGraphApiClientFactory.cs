using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface IGraphApiClientFactory
    {
        IUserGraphApiClient CreateUserGraphApiClient(IAccessTokenManager accessTokenManager, INotifier notifier);

        IGroupGraphApiClient CreateGroupGraphApiClient(IAccessTokenManager accessTokenManager, INotifier notifier);
    }
}
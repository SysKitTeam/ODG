using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface ISharePointServiceFactory
    {
        ISharePointService Create(SimpleUserCredentials credentials, INotifier notifier);
    }
}

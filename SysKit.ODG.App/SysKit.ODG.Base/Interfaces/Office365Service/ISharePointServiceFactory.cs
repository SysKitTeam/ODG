using SysKit.ODG.Base.Authentication;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface ISharePointServiceFactory
    {
        ISharePointService Create(SimpleUserCredentials credentials);
    }
}

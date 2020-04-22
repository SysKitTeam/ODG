using System;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface ISharePointServiceFactory
    {
        ISharePointService Create(SimpleUserCredentials credentials, INotifier notifier);

        /// <summary>
        /// This will temporarily give user admin rights on site so he can execute other actions
        /// </summary>
        IDisposable CreateElevatedScope(SimpleUserCredentials credentials, SiteEntry site);
    }
}

using System;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Office365Service.SharePoint
{
    public class SharePointServiceFactory : ISharePointServiceFactory
    {
        public ISharePointService Create(SimpleUserCredentials credentials, INotifier notifier)
        {
            return new SharePointService(credentials, notifier);
        }

        public IDisposable CreateElevatedScope(SimpleUserCredentials credentials, SiteEntry site)
        {
            return new ElevatedSharePointScope(site, credentials);
        }
    }
}

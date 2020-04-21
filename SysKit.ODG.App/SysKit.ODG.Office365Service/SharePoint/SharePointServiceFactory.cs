using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;

namespace SysKit.ODG.Office365Service.SharePoint
{
    public class SharePointServiceFactory : ISharePointServiceFactory
    {
        public ISharePointService Create(SimpleUserCredentials credentials)
        {
            return new SharePointService(credentials);
        }
    }
}

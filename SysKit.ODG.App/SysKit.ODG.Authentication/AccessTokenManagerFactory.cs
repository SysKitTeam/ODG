using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Authentication
{
    public class AccessTokenManagerFactory : IAccessTokenManagerFactory
    {
        private readonly IAppConfigManager _appConfigManager;
        public AccessTokenManagerFactory(IAppConfigManager appConfigManager)
        {
            _appConfigManager = appConfigManager;
        }

        public IAccessTokenManager CreateAccessTokenManager(SimpleUserCredentials userCredentials, string clientId)
        {
            return  new AccessTokenManager(_appConfigManager, userCredentials, clientId);
        }
    }
}

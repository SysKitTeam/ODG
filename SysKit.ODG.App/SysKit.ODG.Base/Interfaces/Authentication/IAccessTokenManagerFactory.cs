using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Authentication;

namespace SysKit.ODG.Base.Interfaces.Authentication
{
    public interface IAccessTokenManagerFactory
    {
        IAccessTokenManager CreateAccessTokenManager(SimpleUserCredentials userCredentials);
    }
}

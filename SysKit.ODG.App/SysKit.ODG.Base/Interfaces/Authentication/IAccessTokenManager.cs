using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.Authentication;

namespace SysKit.ODG.Base.Interfaces.Authentication
{
    public interface IAccessTokenManager
    {
        Task<AuthToken> GetGraphToken(SimpleUserCredentials userCredentials);
    }
}

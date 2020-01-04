using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Interfaces
{
    public interface IAppConfigManager
    {
        string UserAgent { get; }
        string ClientId { get; }
        string[] Scopes { get; }
        /// <summary>
        /// Password that will be set for new users if one is not defined
        /// </summary>
        string DefaultUserPassword { get; }
    }
}

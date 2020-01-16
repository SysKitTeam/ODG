using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Interfaces
{
    public interface IAppConfigManager
    {
        string UserAgent { get; }
        //string ClientId { get; }
        string[] Scopes { get; }
    }
}

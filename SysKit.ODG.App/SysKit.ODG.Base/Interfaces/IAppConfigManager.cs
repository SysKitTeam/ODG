using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Interfaces
{
    public interface IAppConfigManager
    {
        string ClientId { get; }
        string[] Scopes { get; }
    }
}

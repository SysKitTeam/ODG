using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.Interfaces;

namespace SysKit.ODG.App.Configuration
{
    public class AppConfigManager: IAppConfigManager
    {
        public string ClientId => (string)ConfigurationManager.AppSettings["clientId"];
    }
}

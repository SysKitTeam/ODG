using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.Interfaces;

namespace SysKit.ODG.App.Configuration
{
    // TODO: throw not configured errors if something is empty
    public class AppConfigManager: IAppConfigManager
    {
        public string UserAgent => (string)ConfigurationManager.AppSettings["userAgent"];
        public string ClientId => (string)ConfigurationManager.AppSettings["clientId"];

        private string[] _scopes;

        public string[] Scopes
        {
            get
            {
                if (_scopes == null)
                {
                    var scopes = (string)ConfigurationManager.AppSettings["graphScopes"];
                    _scopes = scopes.Split(new char[] { ' ' }).ToArray();
                }

                return _scopes;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysKit.ODG.App.Configuration
{
    public class AppConfigManager: ConfigurationSection
    {
        public static AppConfigManager Create()
        {
            string section = "CustomSettings";
            ConfigurationManager.RefreshSection(section);

            AppConfigManager config = (AppConfigManager)ConfigurationManager.GetSection(section);
            if (config == null)
            {
                throw new ArgumentException("CustomSettings must be supplied. Check README for detailed instructions");
            }

            return config;
        }

        public override bool IsReadOnly()
        {
            return true;
        }

        [ConfigurationProperty("clientId", IsRequired = true)]
        public string ClientId
        {
            get => (string)this["clientId"];
            set => this["clientId"] = value;
        }
    }
}

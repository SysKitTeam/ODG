using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.XmlTemplate;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class GenerationOptions
    {
        public IAccessTokenManager UserAccessTokenManager { get; }
        public string TenantDomain { get; }
        /// <summary>
        /// If set this password will be used as default password for all new users
        /// </summary>
        public string DefaultPassword { get; set; }

        public XmlODGTemplate Template { get; }

        public GenerationOptions(IAccessTokenManager userAccessTokenManager, string tenantDomain, string defaultPassword,
            XmlODGTemplate template)
        {
            UserAccessTokenManager = userAccessTokenManager;
            TenantDomain = tenantDomain;
            DefaultPassword = defaultPassword;
            Template = template;
        }
    }
}

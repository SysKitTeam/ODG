using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.XmlTemplate;

namespace SysKit.ODG.Base.DTO.Generation.Options
{
    public class GenerationOptions
    {
        public IAccessTokenManager UserAccessTokenManager { get; }
        public string TenantDomain { get; }
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

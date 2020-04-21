using SysKit.ODG.Base.Authentication;
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
        public SimpleUserCredentials UserCredentials { get; }

        public GenerationOptions(IAccessTokenManager userAccessTokenManager, SimpleUserCredentials userCredentials, string tenantDomain, string defaultPassword,
            XmlODGTemplate template)
        {
            UserAccessTokenManager = userAccessTokenManager;
            TenantDomain = tenantDomain;
            DefaultPassword = defaultPassword;
            Template = template;
            UserCredentials = userCredentials;
        }
    }
}

using SysKit.ODG.Base.XmlTemplate.Model;

namespace SysKit.ODG.Base.DTO.Generation.Options
{
    public class UserGenerationOptions
    {
        public string TenantDomain { get; set; }
        /// <summary>
        /// If set this password will be used as default password for all new users
        /// </summary>
        public string DefaultPassword { get; set; }
        public XmlUserCollection UserOptions { get; set; }

        protected UserGenerationOptions()
        {

        }

        public static UserGenerationOptions CreateFromGenerationOptions(GenerationOptions generationOptions)
        {
            return new UserGenerationOptions
            {
                UserOptions = generationOptions.Template.UserCollection,
                DefaultPassword = generationOptions.DefaultPassword,
                TenantDomain = generationOptions.TenantDomain
            };
        }
    }
}

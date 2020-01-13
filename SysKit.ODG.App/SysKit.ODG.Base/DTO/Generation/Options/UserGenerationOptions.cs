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
        public XmlUser[] Users { get; set; }
        public XmlRandomOptions RandomOptions { get; set; }

        protected UserGenerationOptions()
        {

        }

        public static UserGenerationOptions CreateFromGenerationOptions(GenerationOptions generationOptions)
        {
            return new UserGenerationOptions
            {
                Users = generationOptions.Template.Users,
                DefaultPassword = generationOptions.DefaultPassword,
                TenantDomain = generationOptions.TenantDomain,
                RandomOptions = generationOptions.Template.RandomOptions
            };
        }
    }
}

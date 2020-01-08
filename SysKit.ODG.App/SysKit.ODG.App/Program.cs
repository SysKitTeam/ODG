using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.App.Configuration;
using SysKit.ODG.Authentication;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.XmlTemplate;
using SysKit.ODG.Base.XmlTemplate.Model;
using Unity;
using Unity.Lifetime;

namespace SysKit.ODG.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var defaultPassword = "1iH1Z8BwLM";
            var tenantDomain = "M365x314861.onmicrosoft.com";
            var userCredentials = new SimpleUserCredentials("admin@M365x314861.onmicrosoft.com", "1iH1Z8BwLM");
            var testTemplate = new XmlODGTemplate
            {
                UserCollection = new XmlUserCollection
                {
                    RandomOptions = new XmlUserRandomOptions
                    {
                        NumberOfUsers = 5
                    }
                }
            };

            var unityContainer = UnityManager.CreateUnityContainer();
            var accessTokenFactory = unityContainer.Resolve<IAccessTokenManagerFactory>();
            var accessTokenManager = accessTokenFactory.CreateAccessTokenManager(userCredentials);

            var generationOptions = new GenerationOptions(accessTokenManager, tenantDomain, defaultPassword, testTemplate);

            var generationService = unityContainer.Resolve<IGenerationService>();
            generationService.AddGenerationTask(unityContainer.Resolve<IGenerationTask>("userTask"));
            generationService.Start(generationOptions);

            Console.WriteLine("Finished :)");
            Console.ReadLine();
        }
    }
}

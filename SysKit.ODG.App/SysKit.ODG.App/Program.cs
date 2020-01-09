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
using SysKit.ODG.Generation;
using Unity;
using Unity.Lifetime;

namespace SysKit.ODG.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var defaultPassword = "hC7q95955D";
            var tenantDomain = "M365B117306.onmicrosoft.com";
            var userCredentials = new SimpleUserCredentials("admin@M365B117306.onmicrosoft.com", "hC7q95955D");
            var testTemplate = new XmlODGTemplate
            {
                //RandomOptions = new XmlRandomOptions
                //{
                //    NumberOfUsers = 10000
                //},
                Users = new XmlUser[1]
                {
                    new XmlUser
                    {
                        Name = "dino.test.userich1"
                    }
                },
                Groups = new XmlGroup[4]
                {
                    new XmlGroup
                    {
                        Name = "test.grupica"
                    },
                    new XmlUnifiedGroup
                    {
                        Name = "nova.test.grupica1234",
                        DisplayName = "Grupica sa memberima",
                        Members = new []
                        {
                            new XmlMember
                            {
                                Name = "dino.test.userich1"
                            }
                        }
                    },
                    new XmlUnifiedGroup
                    {
                        Name = "unified.test.odg",
                        DisplayName = "Dupla ODG Grupa"
                    },
                    new XmlUnifiedGroup
                    {
                        Name = "unified.test.odg1",
                        DisplayName = "Dupla ODG Grupa"
                    }
                }
            };

            var xmlService = new XmlSpecificationService();
            xmlService.SerializeSpecification(testTemplate, @"C:\Users\dino.kacavenda\test.xml");
            //var template = xmlService.DeserializeSpecification(@"C:\Users\dino.kacavenda\test.xml");

            var unityContainer = UnityManager.CreateUnityContainer();
            var accessTokenFactory = unityContainer.Resolve<IAccessTokenManagerFactory>();
            var accessTokenManager = accessTokenFactory.CreateAccessTokenManager(userCredentials);

            var generationOptions = new GenerationOptions(accessTokenManager, tenantDomain, defaultPassword, testTemplate);

            var generationService = unityContainer.Resolve<IGenerationService>();
            generationService.AddGenerationTask("User Creation", unityContainer.Resolve<IGenerationTask>("userTask"));
            generationService.AddGenerationTask("Group Creation", unityContainer.Resolve<IGenerationTask>("groupTask"));
            generationService.Start(generationOptions).GetAwaiter().GetResult();

            Console.WriteLine("Finished ;)");
            Console.ReadLine();
        }
    }
}

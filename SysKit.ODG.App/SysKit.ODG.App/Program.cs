using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.App.Configuration;
using SysKit.ODG.Authentication;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.XMLSpecification;
using SysKit.ODG.XMLSpecification.Model;
using Unity;
using Unity.Lifetime;

namespace SysKit.ODG.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var userCredentials = new SimpleUserCredentials("admin@M365x314861.onmicrosoft.com", "1iH1Z8BwLM");

            var xmlTemplate = new XmlODGSpecification();

            xmlTemplate.UserCollection = new XmlUserCollection
            {
                Users = new XmlUser[]
                {
                    new XmlUser
                    {
                        DisplayName = "Test name"
                    },
                    new XmlUser()
                }
            }; 

            var generationOptions = new XmlGenerationOptions(userCredentials, xmlTemplate);
            var unityContainer = UnityManager.CreateUnityContainer(userCredentials);

            var generationService = unityContainer.Resolve<IGenerationService>();

            generationService.AddGenerationTask(unityContainer.Resolve<IGenerationTask>("userTask"));
            generationService.Start(generationOptions);
        }
    }
}

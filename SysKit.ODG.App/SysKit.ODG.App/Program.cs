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
using SysKit.ODG.Base.Options;
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
            var randomOptions = new RandomGenerationOptions(userCredentials, "M365x314861.onmicrosoft.com")
            {
                DefaultPassword = "1iH1Z8BwLM",
                UserOptions =
                {
                    NumberOfUsers = 10
                }
            };

            var unityContainer = UnityManager.CreateUnityContainer(userCredentials);

            var generationService = unityContainer.Resolve<IGenerationService>();

            generationService.AddGenerationTask(unityContainer.Resolve<IGenerationTask>("userTask"));
            generationService.Start(randomOptions);
        }
    }
}

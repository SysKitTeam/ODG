using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.App.Configuration;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO;
using SysKit.ODG.Base.Interfaces.Generation;
using Unity;

namespace SysKit.ODG.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var unityContainer = UnityManager.CreateUnityContainer();
            var generationService = unityContainer.Resolve<IGenerationService>();

            var userCredentials = new SimpleUserCredentials("test", "test1");
            var generationOptions = new GenerationOptionsDTO(userCredentials);

            generationService.Start(generationOptions);
        }
    }
}

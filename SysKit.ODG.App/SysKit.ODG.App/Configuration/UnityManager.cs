using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Authentication;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Generation;
using Unity;
using Unity.Injection;

namespace SysKit.ODG.App.Configuration
{
    public class UnityManager
    {
        public static UnityContainer CreateUnityContainer()
        {
            var container = new UnityContainer();

            container.RegisterSingleton<IAppConfigManager, AppConfigManager>();
            container.RegisterSingleton<IGenerationService, GenerationService>();

            return container;
        }
    }
}

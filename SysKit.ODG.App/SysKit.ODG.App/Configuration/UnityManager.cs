using Microsoft.Graph;
using OfficeDevPnP.Core.Framework.Graph;
using SysKit.ODG.Authentication;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Generation;
using SysKit.ODG.Office365Service;
using SysKit.ODG.Office365Service.GraphApiManagers;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace SysKit.ODG.App.Configuration
{
    public class UnityManager
    {
        public static UnityContainer CreateUnityContainer(SimpleUserCredentials userCredentials, string userAgent = null)
        {
            var container = new UnityContainer();

            container.RegisterSingleton<IAppConfigManager, AppConfigManager>();
            container.RegisterInstance<IAccessTokenManager>(new AccessTokenManager(container.Resolve<IAppConfigManager>(), userCredentials), new SingletonLifetimeManager());
            container.RegisterSingleton<IHttpProvider, PnPHttpProvider>(new InjectionConstructor(10, 500, userAgent));

            #region Generation services
            
            container.RegisterSingleton<IGenerationService, GenerationService>();
            
            #endregion Generation services

            #region Office365 services

            container.RegisterSingleton<IGraphServiceCreator, GraphServiceCreator>();
            container.RegisterType<IUserGraphApiClient, UserGraphApiClient>();

            #endregion Office365 services

            return container;
        }
    }
}

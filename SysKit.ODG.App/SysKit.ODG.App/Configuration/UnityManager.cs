using System;
using System.IO;
using AutoMapper;
using Serilog;
using SysKit.ODG.Authentication;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Interfaces.SampleData;
using SysKit.ODG.Common.Interfaces.SampleData;
using SysKit.ODG.Generation;
using SysKit.ODG.Generation.Groups;
using SysKit.ODG.Generation.Sites;
using SysKit.ODG.Generation.Users;
using SysKit.ODG.Office365Service;
using SysKit.ODG.Office365Service.GraphHttpProvider;
using SysKit.ODG.Office365Service.Polly;
using SysKit.ODG.Office365Service.SharePoint;
using SysKit.ODG.SampleData;
using Unity;
using Unity.Lifetime;

namespace SysKit.ODG.App.Configuration
{
    public class UnityManager
    {
        public static UnityContainer CreateUnityContainer()
        {
            var container = new UnityContainer();

            container.addLogging();
            container.RegisterInstance<IMapper>(AutomapperManager.ConfigureMapper(), new SingletonLifetimeManager());
            container.RegisterSingleton<IAppConfigManager, AppConfigManager>();
            container.RegisterSingleton<IAccessTokenManagerFactory, AccessTokenManagerFactory>();
            container.RegisterSingleton<ISampleDataService, SampleDataService>();
            container.RegisterSingleton<IJobHierarchyService, JobHierarchyService>();

            #region Generation services

            container.RegisterType<IGenerationService, GenerationService>();
            container.RegisterType<IGenerationCleanupService, GenerationCleanupService>();
            container.RegisterType<IGenerationTask, UserGenerationTask>("userTask");
            container.RegisterType<IGenerationTask, GroupGenerationTask>("groupTask");
            container.RegisterType<IGenerationTask, SiteGenerationTask>("siteTask");

            // DataGeneration
            container.RegisterType<IUserDataGeneration, UserDataGeneration>();
            container.RegisterType<IGroupDataGeneration, GroupDataGeneration>();
            container.RegisterType<ISiteDataGeneration, SiteDataGeneration>();

            #endregion Generation services

            #region Office365 services

            container.RegisterSingleton<ICustomRetryPolicyFactory, CustomRetryPolicyFactory>();
            // we dont want to make this singelton since we could have DNS problem with static HttpClientHandler (once we transition to .net core and HttpClientFactory this can be mitigated)
            container.RegisterType<IGraphHttpProviderFactory, GraphHttpProviderFactory>(new PerResolveLifetimeManager());
            container.RegisterSingleton<IGraphServiceFactory, GraphServiceFactory>();
            container.RegisterSingleton<IGraphApiClientFactory, GraphApiClientFactory>();
            container.RegisterSingleton<ISharePointServiceFactory, SharePointServiceFactory>();

            #endregion Office365 services

            return container;
        }

    }

    static class Extension
    {
        public static UnityContainer addLogging(this UnityContainer container)
        {
            var logPath = $"{Path.GetTempPath()}ODG\\ODGLog_{DateTime.Now:yyyy-dd-M--HH-mm}.txt";
            var logger = new LoggerConfiguration()
                .WriteTo
                .Console()
                .WriteTo
                .File(logPath, retainedFileCountLimit: 5)
                .CreateLogger();

            container.RegisterInstance<ILogger>(logger, new SingletonLifetimeManager());
            return container;
        }
    }
}

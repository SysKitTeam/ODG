﻿using System;
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
using Unity;
using Unity.Lifetime;

namespace SysKit.ODG.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var unityContainer = UnityManager.CreateUnityContainer();

            var userCredentials = new SimpleUserCredentials("admin@M365x314861.onmicrosoft.com", "1iH1Z8BwLM");
            var generationOptions = new GenerationOptionsDTO(userCredentials);
            var accessTokenManager = new AccessTokenManager(unityContainer.Resolve<IAppConfigManager>(), userCredentials);

            unityContainer.RegisterInstance<IAccessTokenManager>(accessTokenManager, new SingletonLifetimeManager());

            var generationService = unityContainer.Resolve<IGenerationService>();

            generationService.Start(accessTokenManager, generationOptions);
        }
    }
}

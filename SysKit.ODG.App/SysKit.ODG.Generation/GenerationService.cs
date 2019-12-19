using SysKit.ODG.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;

namespace SysKit.ODG.Generation
{
    public class GenerationService: IGenerationService
    {
        private readonly IAppConfigManager _configManager;
        private readonly IUserGraphApiClient _userGraphApiClient;

        private IAccessTokenManager _accessTokenManager;
        private IGenerationOptions _generationOptions;

        public GenerationService(IAppConfigManager configManager, IUserGraphApiClient userGraphApiClient)
        {
            _configManager = configManager;
            _userGraphApiClient = userGraphApiClient;
        }

        public void Start(IAccessTokenManager accessTokenManager, IGenerationOptions generationOptions)
        {
            _accessTokenManager = accessTokenManager;
            _generationOptions = generationOptions;

            _userGraphApiClient.GetAllTenantUsers();

            Console.WriteLine(_configManager.ClientId);
            Console.ReadLine();
        }
    }
}

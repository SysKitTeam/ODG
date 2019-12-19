using SysKit.ODG.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;

namespace SysKit.ODG.Generation
{
    public class GenerationService: IGenerationService
    {
        private readonly IAppConfigManager _configManager;
        private IAccessTokenManager _accessTokenManager;

        private IGenerationOptions _generationOptions;

        public GenerationService(IAppConfigManager configManager)
        {
            _configManager = configManager;
        }

        public void Start(IAccessTokenManager accessTokenManager, IGenerationOptions generationOptions)
        {
            _accessTokenManager = accessTokenManager;
            _generationOptions = generationOptions;

            var firstToken = _accessTokenManager.GetGraphToken().GetAwaiter().GetResult();
            var secondToken = _accessTokenManager.GetGraphToken().GetAwaiter().GetResult();

            Console.WriteLine(_configManager.ClientId);
            Console.ReadLine();
        }
    }
}

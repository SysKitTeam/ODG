using SysKit.ODG.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Interfaces.Generation;

namespace SysKit.ODG.Generation
{
    public class GenerationService: IGenerationService
    {
        private readonly IAppConfigManager _configManager;
        private IGenerationOptions _generationOptions;

        public GenerationService(IAppConfigManager configManager)
        {
            _configManager = configManager;
        }

        public void Start(IGenerationOptions generationOptions)
        {
            _generationOptions = generationOptions;
            Console.WriteLine(_configManager.ClientId);
            Console.ReadLine();
        }
    }
}

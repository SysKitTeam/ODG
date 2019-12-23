using SysKit.ODG.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;
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

        public void Start(IGenerationOptions generationOptions)
        {
            _generationOptions = generationOptions;

            var testUsers = new List<UserEntry>();

            for (var i = 11; i < 20; i++)
            {
                testUsers.Add(new UserEntry
                {
                    AccountEnabled = i > 8,
                    DisplayName = $"Test User {i}",
                    UserPrincipalName = $"testUser{i}@M365x314861.onmicrosoft.com",
                    MailNickname = $"testUser{i}",
                    Password = _configManager.DefaultUserPassword
                });
            }

            _userGraphApiClient.CreateTenantUsers(testUsers).GetAwaiter().GetResult();

            Console.WriteLine(_configManager.ClientId);
            Console.ReadLine();
        }
    }
}

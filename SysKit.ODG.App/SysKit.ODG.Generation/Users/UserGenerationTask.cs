using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;

namespace SysKit.ODG.Generation.Users
{
    public class UserGenerationTask: IGenerationTask
    {
        private readonly IUserDataGeneration _userDataGenerationService;
        private readonly IGraphApiClientFactory _graphApiClientFactory;
        private readonly ILogger _logger;

        public UserGenerationTask(ILogger logger, IUserDataGeneration userDataGenerationService, IGraphApiClientFactory graphApiClientFactory)
        {
            _logger = logger;
            _userDataGenerationService = userDataGenerationService;
            _graphApiClientFactory = graphApiClientFactory;
        }

        public async Task Execute(GenerationOptions options)
        {
            var userGraphApiClient = _graphApiClientFactory.CreateUserGraphApiClient(options.UserAccessTokenManager);
            var userGenerationOptions = UserGenerationOptions.CreateFromGenerationOptions(options);
            var users = _userDataGenerationService.CreateUsers(userGenerationOptions);

            var createdUsers = await userGraphApiClient.CreateTenantUsers(users);
            _logger.Information($"Created {createdUsers.Count}/{users.Count()}");

            // TODO: assign licences
            // TODO: add external users
            // TODO: invite external users
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.DTO.Generation.Results;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Generation.Users
{
    public class UserGenerationTask: IGenerationTask
    {
        private readonly IUserDataGeneration _userDataGenerationService;
        private readonly IGraphApiClientFactory _graphApiClientFactory;

        public UserGenerationTask(IUserDataGeneration userDataGenerationService, IGraphApiClientFactory graphApiClientFactory)
        {
            _userDataGenerationService = userDataGenerationService;
            _graphApiClientFactory = graphApiClientFactory;
        }

        public async Task<IGenerationTaskResult> Execute(GenerationOptions options, INotifier notifier)
        {
            var userGraphApiClient = _graphApiClientFactory.CreateUserGraphApiClient(options.UserAccessTokenManager, notifier);
            var userGenerationOptions = UserGenerationOptions.CreateFromGenerationOptions(options);
            var users = _userDataGenerationService.CreateUsers(userGenerationOptions).ToList();

            var createdUsers = await userGraphApiClient.CreateTenantUsers(users);
            notifier.Info($"Created Users: {createdUsers.CreatedEntries}/{users.Count}; Had Erros: {createdUsers.HadErrors}");

            // TODO: assign licences
            // TODO: add external users
            // TODO: invite external users

            return new UserGenerationTaskResult(createdUsers.CreatedEntries, createdUsers.HadErrors);
        }
    }
}

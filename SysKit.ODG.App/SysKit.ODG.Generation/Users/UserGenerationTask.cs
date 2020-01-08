using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;

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

        public async Task Execute(GenerationOptions options)
        {
            var userGraphApiClient = _graphApiClientFactory.CreateUserGraphApiClient(options.UserAccessTokenManager);
            var userGenerationOptions = UserGenerationOptions.CreateFromGenerationOptions(options);
            var users = _userDataGenerationService.CreateUsers(userGenerationOptions);

            try
            {
                userGraphApiClient.GetAllTenantUsers();
                Console.WriteLine("after api call");
                await userGraphApiClient.CreateTenantUsers(users);
                userGraphApiClient.GetAllTenantUsers();
            }
            catch (Exception e)
            {
                throw;
            }
            

            // TODO: assign licences
            // TODO: add external users
            // TODO: invite external users
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;

namespace SysKit.ODG.Generation.Users
{
    public class UserGenerationTask: IGenerationTask
    {
        private readonly IUserDataGeneration _userDataGenerationService;
        private readonly IUserGraphApiClient _userGraphApiClient;

        public UserGenerationTask(IUserDataGeneration userDataGenerationService, IUserGraphApiClient userGraphApiClient)
        {
            _userDataGenerationService = userDataGenerationService;
            _userGraphApiClient = userGraphApiClient;
        }

        public async Task Execute(IGenerationOptions options)
        {
            var users = _userDataGenerationService.CreateUsers(options);

            try
            {
                await _userGraphApiClient.CreateTenantUsers(users);
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

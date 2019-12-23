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
        private readonly IDataGenerationFactory _dataGenerationFactory;
        private readonly IUserGraphApiClient _userGraphApiClient;

        public UserGenerationTask(IDataGenerationFactory dataGenerationFactory, IUserGraphApiClient userGraphApiClient)
        {
            _dataGenerationFactory = dataGenerationFactory;
            _userGraphApiClient = userGraphApiClient;
        }

        public async Task Execute(IGenerationOptions options)
        {
            var users = _dataGenerationFactory.GetUserData(options);
            await _userGraphApiClient.CreateTenantUsers(users);

            // TODO: assign licences
            // TODO: add external users
            // TODO: invite external users
        }
    }
}

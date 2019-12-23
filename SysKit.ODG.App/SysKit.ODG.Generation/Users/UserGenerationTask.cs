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
        private readonly IUserDataGeneration _userDataGeneration;
        private readonly IUserGraphApiClient _userGraphApiClient;

        public UserGenerationTask(IUserDataGeneration userDataGeneration, IUserGraphApiClient userGraphApiClient)
        {
            _userDataGeneration = userDataGeneration;
            _userGraphApiClient = userGraphApiClient;
        }

        public Task Execute(IGenerationOptions options)
        {
            return null;
        }
    }
}

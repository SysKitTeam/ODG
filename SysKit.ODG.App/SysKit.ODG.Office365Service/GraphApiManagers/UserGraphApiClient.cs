using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using OfficeDevPnP.Core.Utilities;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public class UserGraphApiClient: BaseGraphApiClient, IUserGraphApiClient
    {
        public UserGraphApiClient(IGraphServiceCreator graphServiceCreator) : base(graphServiceCreator)
        {

        }

        public void GetAllTenantUsers()
        {
            var result = _graphServiceClient.Users.Request().Top(100).GetAsync().GetAwaiter().GetResult();
            foreach (var user in result)
            {
               Console.WriteLine(user.DisplayName);
            }
        }
    }
}

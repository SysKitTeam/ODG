using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Graph;
using Newtonsoft.Json;
using OfficeDevPnP.Core.Utilities;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Office365Service.GraphHttpProvider;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public class UserGraphApiClient: BaseGraphApiClient, IUserGraphApiClient
    {
        public UserGraphApiClient(IAccessTokenManager accessTokenManager,
            IGraphHttpProviderFactory graphHttpProviderFactory,
            IGraphServiceCreator graphServiceCreator,
            IMapper autoMapper) : base(accessTokenManager, graphHttpProviderFactory, graphServiceCreator, autoMapper)
        {

        }

        public void GetAllTenantUsers()
        {
            var result = _graphServiceClient.Users.Request().Top(999).GetAsync().GetAwaiter().GetResult();
            Console.WriteLine(result.Count);
            //foreach (var user in result)
            //{
            //   Console.WriteLine(user.DisplayName);
            //}
        }

        public async Task CreateTenantUsers(IEnumerable<UserEntry> users)
        {
            var batchEntries = new List<GraphBatchRequest>();
            foreach (var user in users)
            {
                var graphUser = _autoMapper.Map<UserEntry, User>(user, config => config.AfterMap((src, dest) =>
                {
                    dest.PasswordProfile = new PasswordProfile
                    {
                        Password = src.Password,
                        ForceChangePasswordNextSignIn = false
                    };
                }));

                batchEntries.Add(new GraphBatchRequest(graphUser.UserPrincipalName, "users", HttpMethod.Post, graphUser));
            }

            var tokenRessult = await _accessTokenManager.GetGraphToken();
            var result = await _httpProvider.SendBatchAsync(batchEntries, tokenRessult.Token);
        }
    }
}

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

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public class UserGraphApiClient: BaseGraphApiClient, IUserGraphApiClient
    {
        public UserGraphApiClient(IAppConfigManager appConfigManager, IGraphServiceCreator graphServiceCreator, IMapper autoMapper, IGraphHttpProvider httpProvider, IAccessTokenManager accessTokenManager) : base(appConfigManager, accessTokenManager, httpProvider, graphServiceCreator, autoMapper)
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

        public async Task CreateTenantUsers(IEnumerable<UserEntry> users)
        {
            var batch = new BatchRequestContent();

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

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/users");
                httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(graphUser), Encoding.UTF8, "application/json");
                //httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
                //    _accessTokenManager.GetGraphToken().GetAwaiter().GetResult().Token);

                var batchStep = new BatchRequestStep(user.UserPrincipalName, httpRequestMessage);
                batch.AddBatchRequestStep(batchStep);
            }

            // Send batch request with BatchRequestContent.
            var batchHttpRequest = new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/$batch") { Content = batch };
            //batchHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
            //    _accessTokenManager.GetGraphToken().GetAwaiter().GetResult().Token);

            HttpResponseMessage batchRequest = await _httpProvider.SendAsync(batchHttpRequest);

            var batchResponseContent = new BatchResponseContent(batchRequest);
            Dictionary<string, HttpResponseMessage> responses = await batchResponseContent.GetResponsesAsync();

            foreach (var response in responses)
            {
                var id = 1;
            }
        }
    }
}

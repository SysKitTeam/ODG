using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public abstract class BaseGraphApiClient
    {
        public static string UserAgent { get; set; }

        /// <summary>
        /// Used to ping v1 Graph API
        /// </summary>
        protected readonly IGraphServiceClient _graphServiceClient;
        /// <summary>
        /// Used to ping beta endpoint
        /// </summary>
        protected readonly IGraphServiceClient _graphServiceBetaClient;

        protected BaseGraphApiClient(IAccessTokenManager tokenManager)
        {
            _graphServiceClient = createGraphServiceClient(tokenManager, false);
            _graphServiceBetaClient = createGraphServiceClient(tokenManager, true);
        }

        #region Request execution helpers

        /// <summary>
        /// Used to execute Graph API requests. This method handles throttling
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// /// <param name="retryCount"></param>
        /// <returns></returns>
        protected async Task<T> executeRequestAsync<T>(Func<Task<T>> request, int retryCount = 5)
        {
            try
            {
                return await request().ConfigureAwait(false);
            }
            catch (ServiceException sex)
            {
                //if (((int)sex.StatusCode == 429 || (int)sex.StatusCode == 503) && retryCount > 0)
                //{
                //    var retryAfterValueInMiliseconds = _httpClientFactory.GetRetryAfterValue(sex.ResponseHeaders);
                //    EventLogManager.WriteEntry($"Requests have been throtthled! retrying after {retryAfterValueInMiliseconds / 1000} seconds", EventLogEntryType.Warning);
                //    await Task.Delay(retryAfterValueInMiliseconds).ConfigureAwait(false);
                //    return await executeRequestAsync(request, retryCount - 1).ConfigureAwait(false);
                //}

                throw;
            }
        }

        #endregion

        /// <summary>
        /// Creates service client that is used for pinging Graph API
        /// </summary>
        /// <param name="tokenRetriever"></param>
        /// <param name="useBetaEndpoint"></param>
        /// <returns></returns>
        private IGraphServiceClient createGraphServiceClient(IAccessTokenManager tokenRetriever, bool useBetaEndpoint = false)
        {
            var baseUrl = "https://graph.microsoft.com/v1.0";
            if (useBetaEndpoint)
            {
                baseUrl = "https://graph.microsoft.com/beta";
            }

            return new GraphServiceClient(baseUrl, new DelegateAuthenticationProvider(
                request =>
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenRetriever.GetGraphToken().GetAwaiter().GetResult().Token);
                    if (!string.IsNullOrEmpty(UserAgent))
                    {
                        request.Headers.Add("User-Agent", UserAgent);
                    }
                    return Task.FromResult(0);
                }));
        }
    }
}

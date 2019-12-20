using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Office365Service
{
    public interface IGraphServiceCreator
    {
        /// <summary>
        /// Creates service client that is used for pinging Graph API
        /// </summary>
        /// <param name="tokenRetriever"></param>
        /// <param name="httpProvider"></param>
        /// <param name="useBetaEndpoint"></param>
        /// <returns></returns>
        IGraphServiceClient CreateGraphServiceClient(bool useBetaEndpoint = false);
    }

    public class GraphServiceCreator : IGraphServiceCreator
    {
        // TODO: real agent
        private const string USER_AGENT = null;
        private readonly IAccessTokenManager _accessTokenManager;
        private readonly IHttpProvider _httpProvider;

        public GraphServiceCreator(IAccessTokenManager accessTokenManager, IHttpProvider httpProvider)
        {
            _accessTokenManager = accessTokenManager;
            _httpProvider = httpProvider;
        }

        /// <inheritdoc />
        public IGraphServiceClient CreateGraphServiceClient(bool useBetaEndpoint = false)
        {
            var baseUrl = "https://graph.microsoft.com/v1.0";
            if (useBetaEndpoint)
            {
                baseUrl = "https://graph.microsoft.com/beta";
            }

            return new GraphServiceClient(baseUrl, new DelegateAuthenticationProvider(
                request =>
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessTokenManager.GetGraphToken().GetAwaiter().GetResult().Token);
                    if (!string.IsNullOrEmpty(USER_AGENT))
                    {
                        request.Headers.Add("User-Agent", USER_AGENT);
                    }
                    return Task.FromResult(0);
                }), _httpProvider);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Office365Service.GraphHttpProvider;

namespace SysKit.ODG.Office365Service
{
    public interface IGraphServiceCreator
    {
        /// <summary>
        /// Creates service client that is used for pinging Graph API
        /// </summary>
        /// <param name="accessTokenManager"></param>
        /// <param name="useBetaEndpoint"></param>
        /// <returns></returns>
        IGraphServiceClient CreateGraphServiceClient(IAccessTokenManager accessTokenManager, bool useBetaEndpoint = false);
    }

    public class GraphServiceCreator : IGraphServiceCreator
    {
        private readonly IGraphHttpProvider _graphHttpProvider;
        public GraphServiceCreator(IGraphHttpProvider graphHttpProvider)
        {
            _graphHttpProvider = graphHttpProvider;
        }

        /// <inheritdoc />
        public IGraphServiceClient CreateGraphServiceClient(IAccessTokenManager accessTokenManager, bool useBetaEndpoint = false)
        {
            var baseUrl = "https://graph.microsoft.com/v1.0";
            if (useBetaEndpoint)
            {
                baseUrl = "https://graph.microsoft.com/beta";
            }

            return new GraphServiceClient(baseUrl, new DelegateAuthenticationProvider(
                request =>
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenManager.GetGraphToken().GetAwaiter().GetResult().Token);
                    return Task.FromResult(0);
                }), _graphHttpProvider);
        }
    }
}

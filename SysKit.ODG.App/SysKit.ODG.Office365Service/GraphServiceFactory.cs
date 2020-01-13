using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Office365Service.GraphHttpProvider;

namespace SysKit.ODG.Office365Service
{
    public class GraphServiceFactory : IGraphServiceFactory
    {
        private readonly IGraphHttpProviderFactory _graphHttpProviderFactory;
        public GraphServiceFactory(IGraphHttpProviderFactory graphHttpProviderFactory)
        {
            _graphHttpProviderFactory = graphHttpProviderFactory;
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
                }), _graphHttpProviderFactory.CreateHttpProvider());
        }
    }
}

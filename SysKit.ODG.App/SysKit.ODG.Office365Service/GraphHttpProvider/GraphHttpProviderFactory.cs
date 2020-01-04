using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Graph;
using SysKit.ODG.Office365Service.GraphHttpProvider.Handlers;
using SysKit.ODG.Office365Service.Utils;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    public class GraphHttpProviderFactory: IGraphHttpProviderFactory
    {
        public IGraphHttpProvider CreateHttpProvider(int retryCount, string userAgent = null)
        {
            var requestHandler = new HttpClientHandler();
            var loggingHandler = new LoggingHandler();

            var requestPipeline = GraphClientFactory.CreatePipeline(new DelegatingHandler[] {loggingHandler}, requestHandler);
            return new GraphHttpProvider(requestPipeline, retryCount, userAgent);
        }
    }
}

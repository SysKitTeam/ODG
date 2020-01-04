using System;
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
            var httpProvider = new GraphHttpProvider(requestPipeline, retryCount, userAgent);

            // we will handle timeout with a delegation handler because HttpClient attaches user cancellation token with his inner token and then polly retry doesn't work as intended
            //httpProvider.OverallTimeout = TimeSpan.MaxValue;

            return httpProvider;
        }
    }
}

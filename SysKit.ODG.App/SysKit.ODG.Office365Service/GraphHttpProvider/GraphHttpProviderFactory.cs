using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Office365Service.GraphHttpProvider.Handlers;
using SysKit.ODG.Office365Service.Utils;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    public class GraphHttpProviderFactory: IGraphHttpProviderFactory
    {
        private readonly IAppConfigManager _configManager;

        private readonly object _lock = new object();
        /// <summary>
        /// We want to cache(similar problem to HttpClient)
        /// </summary>
        private IGraphHttpProvider _cachedProvider;

        public GraphHttpProviderFactory(IAppConfigManager configManager)
        { 
            _configManager = configManager;
        }

        public IGraphHttpProvider CreateHttpProvider()
        {
            if (_cachedProvider == null)
            {
                lock (_lock)
                {
                    if (_cachedProvider == null)
                    {
                        _cachedProvider = createGraphHttpProvider();
                    }
                }
            }

            return _cachedProvider;
        }

        private IGraphHttpProvider createGraphHttpProvider()
        {
            var requestHandler = new HttpClientHandler();
            var compressionHandler = new CompressionHandler();
            var loggingHandler = new LoggingHandler();

            var requestPipeline = GraphClientFactory.CreatePipeline(new DelegatingHandler[] { loggingHandler, compressionHandler }, requestHandler);
            var httpProvider = new GraphHttpProvider(requestPipeline);

            return httpProvider;
        }
    }
}

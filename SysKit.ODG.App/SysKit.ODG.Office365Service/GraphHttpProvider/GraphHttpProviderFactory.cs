using System;
using System.Net.Http;
using Microsoft.Graph;
using Serilog;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Office365Service.GraphHttpProvider.Handlers;
using SysKit.ODG.Office365Service.Polly;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    public class GraphHttpProviderFactory: IGraphHttpProviderFactory
    {
        private readonly IAppConfigManager _configManager;
        private readonly ILogger _logger;
        private readonly ICustomRetryPolicyFactory _customRetryPolicyFactory;

        private readonly object _lock = new object();
        /// <summary>
        /// We want to cache(similar problem to HttpClient)
        /// </summary>
        private IGraphHttpProvider _cachedProvider;

        public GraphHttpProviderFactory(IAppConfigManager configManager, ILogger logger, ICustomRetryPolicyFactory customRetryPolicyFactory)
        { 
            _configManager = configManager;
            _logger = logger;
            _customRetryPolicyFactory = customRetryPolicyFactory;
        }

        /// <inheritdoc />
        public IGraphHttpProvider CreateHttpProvider(HttpMessageHandler finalHandler = null)
        {
            if (_cachedProvider == null)
            {
                lock (_lock)
                {
                    if (_cachedProvider == null)
                    {
                        _cachedProvider = createGraphHttpProvider(finalHandler ?? new HttpClientHandler());
                    }
                }
            }

            return _cachedProvider;
        }

        private IGraphHttpProvider createGraphHttpProvider(HttpMessageHandler finalHandler)
        {
            var retryPolicy = _customRetryPolicyFactory.CreateRetryPolicy();
            var userAgentHandler = new UserAgentHandler(_configManager.UserAgent);
            var compressionHandler = new CompressionHandler();
            var loggingHandler = new LoggingHandler(_logger);
            var retryHandler = new CustomRetryHandler(retryPolicy);

            var requestPipeline = GraphClientFactory.CreatePipeline(new DelegatingHandler[] { loggingHandler, userAgentHandler, compressionHandler, retryHandler }, finalHandler);
            var httpProvider = new GraphHttpProvider(requestPipeline, retryPolicy);
            httpProvider.OverallTimeout = TimeSpan.FromMinutes(15);

            return httpProvider;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Serilog;

namespace SysKit.ODG.Office365Service.GraphHttpProvider.Handlers
{
    public class LoggingHandler: DelegatingHandler
    {
        private readonly ILogger _logger;
        public LoggingHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            try
            {
                _logger.Verbose($"Started request for {request.RequestUri.AbsolutePath} as {DateTime.Now}");
                var response = await base.SendAsync(request, token);
                _logger.Verbose($"Ended request for {request.RequestUri.AbsolutePath} as {DateTime.Now}");
                return response;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error while executing request: {request?.RequestUri}");
                throw;
            }
        }
    }
}

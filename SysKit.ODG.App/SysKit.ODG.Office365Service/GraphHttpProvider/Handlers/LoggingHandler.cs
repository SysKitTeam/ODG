using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace SysKit.ODG.Office365Service.GraphHttpProvider.Handlers
{
    public class LoggingHandler: DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Console.WriteLine($"message: {request.RequestUri.AbsolutePath} {request?.Content is BatchRequestContent}");
            var response = await base.SendAsync(request, token);
            Console.WriteLine($"message for me after: {request.RequestUri.AbsolutePath}");
            return response;
        }
    }
}

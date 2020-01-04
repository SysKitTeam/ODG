using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace SysKit.ODG.Office365Service.GraphHttpProvider.Handlers
{
    public class RetryHandler: DelegatingHandler
    {
        AsyncPolicy policy;

        public RetryHandler()
        {
            policy = Policy
                .Handle<TimeoutException>()
                .RetryAsync(onRetry: OnRetry);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return await policy.ExecuteAsync(
                async context =>
                {
                    return await base.SendAsync(request, cancellationToken);
                }, new Dictionary<string, object>());
        }

        private void OnRetry(Exception ex, int retryCount, Context context)
        {
            Console.WriteLine("retry");
        }
    }
}

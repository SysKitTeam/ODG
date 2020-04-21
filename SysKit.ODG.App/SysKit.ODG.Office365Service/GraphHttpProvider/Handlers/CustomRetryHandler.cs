using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SysKit.ODG.Office365Service.Polly;

namespace SysKit.ODG.Office365Service.GraphHttpProvider.Handlers
{
    public class CustomRetryHandler : DelegatingHandler
    {
        ICustomRetryPolicy _retryPolicy;

        public CustomRetryHandler(ICustomRetryPolicy customRetryPolicy)
        {
            _retryPolicy = customRetryPolicy;
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return await _retryPolicy.ExecuteAsync(() => base.SendAsync(request, cancellationToken));
        }
    }
}

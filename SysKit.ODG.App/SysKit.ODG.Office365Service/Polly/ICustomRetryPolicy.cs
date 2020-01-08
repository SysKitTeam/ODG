using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SysKit.ODG.Office365Service.Polly
{
    public interface ICustomRetryPolicy
    {
        Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> requestFunction);
        Task ExecuteAsync(Func<Task> requestFunction, string requestUri);

        TimeSpan? GetRetryAfterValueFromResponseHeader(HttpResponseHeaders headers);
        bool IsResponseThrottled(HttpResponseMessage response);
    }
}

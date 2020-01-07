using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Polly.Wrap;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.Utils;

namespace SysKit.ODG.Office365Service.Polly
{
    public class CustomRetryPolicy
    {
        private AsyncPolicyWrap<HttpResponseMessage> _policy;

        /// <summary>
        /// If set to -1 it will retry forever
        /// </summary>
        /// <param name="maxRetryCount"></param>
        public CustomRetryPolicy()
        {
            var maxRetryCount = 2;
            var maxFailedRequestsForCircuitbreaker = 3;

            var retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(isResponseThrottled)
                .WaitAndRetryAsync(retryCount: maxRetryCount, sleepDurationProvider: calculateRetryTime,
                    onRetryAsync: onRetryAsync);

            var circuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(isResponseThrottled)
                .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: maxFailedRequestsForCircuitbreaker,
                    durationOfBreak: TimeSpan.FromMinutes(1),
                    onBreak: onBreak, dummy, dummy);

            _policy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
        }

        private void onBreak(DelegateResult<HttpResponseMessage> response, TimeSpan timeout)
        {
            Console.WriteLine($"Request {response.Result.RequestMessage.RequestUri} broke the circuit");
        }

        private void dummy()
        {
            // we need to supply this
        }

        private async Task onRetryAsync(DelegateResult<HttpResponseMessage> responseMessage, TimeSpan sleepDuration, int retryAttempt, Context context)
        {
            Console.WriteLine($"Request {responseMessage.Result.RequestMessage.RequestUri} was throttled");
        }

        private bool isResponseThrottled(HttpResponseMessage response)
        {
            return (int) response.StatusCode == 429 || (int) response.StatusCode == 503;
        }

        private TimeSpan calculateRetryTime(int retryAttempt, DelegateResult<HttpResponseMessage> responseMessage, Context context)
        {
            var responseTime = getRetryAfterValue(retryAttempt, responseMessage.Result.Headers);
            return responseTime;
        }

        private TimeSpan getRetryAfterValue(int retryAttempt, HttpResponseHeaders headers)
        {
            if (headers != null && headers.TryGetValues("Retry-After", out var values) && values != null &&
                Int32.TryParse(values.First(), out var retryAfterValue))
            {
                // we want jitter so if multiple requests throttle with same value we don't go at the same time
                var tenSecondsInMilliseconds = Convert.ToInt32(new TimeSpan(0, 0, 10).TotalMilliseconds);
                return TimeSpan.FromSeconds(retryAfterValue) + TimeSpan.FromMilliseconds(tenSecondsInMilliseconds);
            }

            var oneMinuteInMilliseconds = Convert.ToInt32(new TimeSpan(0, 1, 0).TotalMilliseconds);
            return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                   TimeSpan.FromMilliseconds(RandomThreadSafeGenerator.Next(oneMinuteInMilliseconds));
        }

        public Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> requestFunction)
        {
            return _policy.ExecuteAsync(requestFunction);
        }
    }
}

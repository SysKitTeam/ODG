using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Polly;
using Polly.Wrap;
using Serilog;
using Serilog.Events;
using SysKit.ODG.Base.Utils;

namespace SysKit.ODG.Office365Service.Polly
{
    public class CustomRetryPolicy: ICustomRetryPolicy
    {
        private readonly ILogger _logger;
        private readonly AsyncPolicyWrap<HttpResponseMessage> _policy;

        public CustomRetryPolicy(ILogger logger)
        {
            _logger = logger;
            var maxRetryCount = 2;
            var maxFailedRequestsForCircuitbreaker = 4;

            var retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(isResponseThrottled)
                .WaitAndRetryAsync(retryCount: maxRetryCount, sleepDurationProvider: calculateRetryTime,
                    onRetryAsync: onRetryAsync);

            var circuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(isResponseThrottled)
                .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: maxFailedRequestsForCircuitbreaker,
                    durationOfBreak: TimeSpan.FromSeconds(10),
                    onBreak: onBreak, dummy, dummy);

            _policy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
        }

        private void onBreak(DelegateResult<HttpResponseMessage> response, TimeSpan timeout)
        {
            _logger.Warning($"Request {response.Result.RequestMessage.RequestUri} broke the circuit");
        }

        private void dummy()
        {
            _logger.Warning("inside dummy");
        }

        private async Task onRetryAsync(DelegateResult<HttpResponseMessage> responseMessage, TimeSpan sleepDuration, int retryAttempt, Context context)
        {
            _logger.Warning($"Request {responseMessage.Result.RequestMessage.RequestUri} was throttled");
        }

        private bool isResponseThrottled(HttpResponseMessage response)
        {
            return true;
            return (int) response.StatusCode == 429 || (int) response.StatusCode == 503;
        }

        private TimeSpan calculateRetryTime(int retryAttempt, DelegateResult<HttpResponseMessage> responseMessage, Context context)
        {
            var retryTime = getRetryAfterValue(retryAttempt, responseMessage.Result.Headers);
            _logger.Warning($"Request {responseMessage.Result.RequestMessage.RequestUri} was throttled (attempt: {retryAttempt}). Timeout: {retryTime.TotalSeconds}s");
            return TimeSpan.FromSeconds(5);
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

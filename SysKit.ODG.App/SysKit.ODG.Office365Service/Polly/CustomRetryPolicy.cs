using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Polly;
using Polly.Wrap;
using Serilog;
using Serilog.Events;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.Utils;

namespace SysKit.ODG.Office365Service.Polly
{
    public class CustomRetryPolicy: ICustomRetryPolicy
    {
        private readonly ILogger _logger;
        private readonly AsyncPolicyWrap _policy;
        private const string CTX_URL = "url_key";

        public CustomRetryPolicy(ILogger logger)
        {
            _logger = logger;
            var maxRetryCount = 3;
            var maxFailedRequestsForCircuitbreaker = 4;

            var retryPolicy = Policy
                .Handle<ThrottleException>()
                .WaitAndRetryAsync(retryCount: maxRetryCount, sleepDurationProvider: calculateRetryTime,
                    onRetryAsync: onRetryAsync);

            var circuitBreakerPolicy = Policy
                .Handle<ThrottleException>()
                .CircuitBreakerAsync(maxFailedRequestsForCircuitbreaker,
                    durationOfBreak: TimeSpan.FromMinutes(2),
                    onBreak, onReset);

            _policy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
        }

        #region Interface

        public async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> requestFunction)
        {
            return await _policy.ExecuteAsync(async (context) =>
            {
                var result = await requestFunction();
                context[CTX_URL] = result.RequestMessage.RequestUri;

                if (isResponseThrottled(result))
                {
                    //// Fix for hanging polly: https://github.com/aspnet/Extensions/issues/1700#issuecomment-537612449
                    result.Dispose();
                    throw new ThrottleException(getRetryAfterValueFromResponseHeader(result.Headers));
                }

                return result;
            }, new Dictionary<string, object>());

        }

        #endregion Interface

        #region Configuration

        private void onReset(Context context)
        {
            _logger.Warning($"Requests are reset. Last request: {getRequestUrlFromContext(context)}");
        }

        private void onBreak(Exception exception, TimeSpan sleep, Context context)
        {
            _logger.Warning($"Requests are disabled for {sleep.TotalSeconds}s. Last request: {getRequestUrlFromContext(context)}");
        }

        private async Task onRetryAsync(Exception error, TimeSpan sleepDuration, int retryAttempt, Context context)
        {
            _logger.Warning($"Request: {getRequestUrlFromContext(context)} was throttled (attempt {retryAttempt}). Throttle time: {sleepDuration.TotalSeconds}s");
        }

        private TimeSpan calculateRetryTime(int retryAttempt, Exception error, Context context)
        {
            var headerRetryValue = error is ThrottleException throttleException ? throttleException.Timeout : null;
            var oneMinuteInMilliseconds = Convert.ToInt32(new TimeSpan(0, 1, 0).TotalMilliseconds);
            var throttleValue = headerRetryValue ?? TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                                TimeSpan.FromMilliseconds(RandomThreadSafeGenerator.Next(oneMinuteInMilliseconds));

            return throttleValue;
        }

        #endregion Configuration


        #region Helpers

        private TimeSpan? getRetryAfterValueFromResponseHeader(HttpResponseHeaders headers)
        {
            if (headers != null && headers.TryGetValues("Retry-After", out var values) && values != null &&
                Int32.TryParse(values.First(), out var retryAfterValue))
            {
                // we want jitter so if multiple requests throttle with same value we don't go at the same time
                var tenSecondsInMilliseconds = Convert.ToInt32(new TimeSpan(0, 0, 10).TotalMilliseconds);
                return TimeSpan.FromSeconds(retryAfterValue) + TimeSpan.FromMilliseconds(tenSecondsInMilliseconds);
            }

            return null;
        }

        private bool isResponseThrottled(HttpResponseMessage response)
        {
            return (int)response.StatusCode == 429 || (int)response.StatusCode == 503;
        }

        private string getRequestUrlFromContext(Context context)
        {
            if (context.TryGetValue(CTX_URL, out object url) && url is Uri uriObject)
            {
                return uriObject.AbsolutePath;
            }

            return "";
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
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

        public CustomRetryPolicy(ILogger logger, CustomRetryPolicySettings settings)
        {
            _logger = logger;
            var maxRetryCount = settings.MaxRetryCount;
            var maxFailedRequestsForCircuitbreaker = settings.MaxConsecutiveThrottledRequests;

            var retryPolicy = Policy
                .Handle<ThrottleException>()
                .Or<BrokenCircuitException>()
                .WaitAndRetryAsync(retryCount: maxRetryCount, sleepDurationProvider: settings.CalculateRetryFunc,
                    onRetryAsync: onRetryAsync);

            var circuitBreakerPolicy = Policy
                .Handle<ThrottleException>()
                .CircuitBreakerAsync(maxFailedRequestsForCircuitbreaker,
                    durationOfBreak: settings.DurationOfCircuitBreak,
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

                if (IsResponseThrottled(result))
                {
                    //// Fix for hanging polly: https://github.com/aspnet/Extensions/issues/1700#issuecomment-537612449
                    result.Dispose();
                    throw new ThrottleException(GetRetryAfterValueFromResponseHeader(result.Headers));
                }

                return result;
            }, new Dictionary<string, object>());

        }

        public Task ExecuteAsync(Func<Task> requestFunction, string requestUri)
        {
            return _policy.ExecuteAsync((context) =>
            {
                context[CTX_URL] = requestUri;
                return requestFunction();
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

        #endregion Configuration


        #region Helpers

        public TimeSpan? GetRetryAfterValueFromResponseHeader(HttpResponseHeaders headers)
        {
            if (headers != null && headers.TryGetValues("Retry-After", out var values) && values != null &&
                Int32.TryParse(values.First(), out var retryAfterValue))
            {
                return TimeSpan.FromSeconds(retryAfterValue);
            }

            return null;
        }

        public bool IsResponseThrottled(HttpResponseMessage response)
        {
            return (int)response.StatusCode == 429 || (int)response.StatusCode == 503;
        }

        private string getRequestUrlFromContext(Context context)
        {
            if (context.TryGetValue(CTX_URL, out object url))
            {
                if (url is Uri uriObject)
                {
                    return uriObject.AbsolutePath;
                }

                return $"{url}";
            }

            return "";
        }

        #endregion
    }

    public class CustomRetryPolicySettings
    {
        /// <summary>
        /// Max number of retries if request is throttled
        /// </summary>
        public int MaxRetryCount { get; private set; }

        /// <summary>
        /// When this number is hit, all requests will fail until system gets back to normal (circuit breaker pattern)
        /// </summary>
        public int MaxConsecutiveThrottledRequests { get; private set; }

        /// <summary>
        /// If MaxConsecutiveThrottledRequests is hit, duration before requests can pass (circuit is open)
        /// </summary>
        public TimeSpan DurationOfCircuitBreak { get; private set; }

        /// <summary>
        /// Used to calculate throttle timeout. First param is retry count, second is exception nad third is the Polly context
        /// </summary>
        public Func<int, Exception, Context, TimeSpan> CalculateRetryFunc { get; private set; }

        public CustomRetryPolicySettings(int maxRetryCount, 
            int maxConsecutiveThrottledRequests,
            TimeSpan durationOfCircuitBreak,
            Func<int, Exception, Context, TimeSpan> calculateRetryFunc)
        {
            MaxRetryCount = maxRetryCount;
            MaxConsecutiveThrottledRequests = maxConsecutiveThrottledRequests;
            DurationOfCircuitBreak = durationOfCircuitBreak;
            CalculateRetryFunc = calculateRetryFunc;
        }
    }
}

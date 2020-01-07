using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using Serilog;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.Utils;

namespace SysKit.ODG.Office365Service.Polly
{
    public interface ICustomRetryPolicyFactory
    {
        ICustomRetryPolicy CreateRetryPolicy();

        ICustomRetryPolicy CreateRetryPolicy(CustomRetryPolicySettings settings);
    }

    public class CustomRetryPolicyFactory : ICustomRetryPolicyFactory
    {
        private readonly ILogger _logger;
        public CustomRetryPolicyFactory(ILogger logger)
        {
            _logger = logger;
        }

        public ICustomRetryPolicy CreateRetryPolicy()
        {
            return CreateRetryPolicy(new CustomRetryPolicySettings(5, 3, TimeSpan.FromMinutes(2), CalculateRetryTime));
        }

        public ICustomRetryPolicy CreateRetryPolicy(CustomRetryPolicySettings settings)
        {
            return new CustomRetryPolicy(_logger, settings);
        }

        public static TimeSpan CalculateRetryTime(int retryAttempt, Exception error, Context context)
        {
            var oneMinuteInMilliseconds = Convert.ToInt32(new TimeSpan(0, 1, 0).TotalMilliseconds);
            var threeMinutesInMilliseconds = Convert.ToInt32(new TimeSpan(0, 3, 0).TotalMilliseconds);

            if (error is BrokenCircuitException)
            {
                // too much ThrottleExceptions, time to wait
                return TimeSpan.FromMinutes(5) + TimeSpan.FromMilliseconds(RandomThreadSafeGenerator.Next(oneMinuteInMilliseconds, threeMinutesInMilliseconds));
            }

            var headerRetryValue = error is ThrottleException throttleException ? throttleException.Timeout : null;
            var throttleValue = (headerRetryValue ?? TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))) +
                                TimeSpan.FromMilliseconds(RandomThreadSafeGenerator.Next(oneMinuteInMilliseconds, threeMinutesInMilliseconds));

            return throttleValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace SysKit.ODG.Office365Service.Polly
{
    public interface ICustomRetryPolicyFactory
    {
        ICustomRetryPolicy CreateRetryPolicy();
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
            return new CustomRetryPolicy(_logger);
        }
    }
}

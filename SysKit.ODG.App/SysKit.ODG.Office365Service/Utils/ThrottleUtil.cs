using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SysKit.ODG.Office365Service.Utils
{
    public static class ThrottleUtil
    {
        private static readonly Random _random = new Random();
        public static int GetRetryAfterValue(HttpResponseHeaders headers)
        {
            if (headers != null && headers.TryGetValues("Retry-After", out var values) && values != null && Int32.TryParse(values.First(), out var retryAfterValue))
            {
                return retryAfterValue * 1000;
            }

            return 2000 + _random.Next(1000);
        }
    }
}

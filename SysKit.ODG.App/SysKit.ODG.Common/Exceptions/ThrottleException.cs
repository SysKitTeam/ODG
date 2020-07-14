using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Exceptions
{
    /// <summary>
    /// Thrown if Graph API/CSOM is throttled
    /// </summary>
    public class ThrottleException: Exception
    {
        public readonly TimeSpan? Timeout;

        public ThrottleException(TimeSpan? timeout)
        {
            Timeout = timeout;
        }
    }
}

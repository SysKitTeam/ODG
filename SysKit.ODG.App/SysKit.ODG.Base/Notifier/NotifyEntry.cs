using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Notifier
{
    public class NotifyEntry
    {
        public string CorrelationId { get; }
        
        /// <summary>
        /// Simple message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets Log Exception
        /// </summary>
        public Exception Exception { get; set; }

        public NotifyEntry(string correlationId, string message)
        {
            CorrelationId = correlationId;
            Message = message;
        }

        public NotifyEntry(string correlationId, Exception exception, string message = null): this(correlationId, message)
        {
            Exception = exception;
        }
    }
}

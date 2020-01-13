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

        public NotifyEntry(string correlationId, string message)
        {
            CorrelationId = correlationId;
            Message = message;
        }
    }
}

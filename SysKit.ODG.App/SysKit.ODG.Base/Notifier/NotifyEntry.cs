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
        /// Used to add additional data
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; }

        public NotifyEntry(string correlationId): this(correlationId, null)
        {

        }

        public NotifyEntry(string correlationId, string message)
        {
            CorrelationId = correlationId;
            Message = message;
            AdditionalData = new Dictionary<string, object>();
        }
    }
}

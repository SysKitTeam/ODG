using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Notifier
{
    public class ExceptionEntry: NotifyEntry
    {
        public Exception Error { get; set; }
        public ExceptionEntry(string correlationId) : base(correlationId)
        {
        }

        public ExceptionEntry(string correlationId, string message) : base(correlationId, message)
        {
        }
    }
}

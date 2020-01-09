using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Notifier
{
    public class ProgressEntry: NotifyEntry
    {
        public int TotalCount { get; set; }
        public int CurrentProgress { get; set; }

        public ProgressEntry(string correlationId) : base(correlationId)
        {
        }

        public ProgressEntry(string correlationId, string message) : base(correlationId, message)
        {

        }
    }
}

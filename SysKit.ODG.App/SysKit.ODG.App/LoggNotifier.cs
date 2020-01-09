using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.App
{
    /// <summary>
    /// Basic implementation of tracking generation progress
    /// </summary>
    public class LoggNotifier: INotifier
    {
        private readonly ILogger _logger;
        public LoggNotifier(ILogger logger)
        {
            _logger = logger;
        }

        public void Notify(NotifyEntry entry)
        {
            if (entry is ProgressEntry progressEntry)
            {
                _logger.Information($"Operation: {progressEntry.CorrelationId}. Progress: {progressEntry.CurrentProgress}/{progressEntry.TotalCount}");
                return;
            }

            _logger.Information($"Operation: {entry.CorrelationId}. Message: {entry.Message}");
        }
    }
}

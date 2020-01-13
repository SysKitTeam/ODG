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
            _logger.Information($"Operation: {entry.CorrelationId}. Message: {entry.Message}");
        }
    }
}

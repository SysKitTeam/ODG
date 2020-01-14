using System;
using System.Timers;
using Microsoft.Graph;
using Serilog;
using SysKit.ODG.Base.Enums;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.App
{
    /// <summary>
    /// Basic implementation of tracking generation progress
    /// </summary>
    public class LoggNotifier: INotifier
    {
        private readonly ILogger _logger;
        private readonly LogLevelEnum _logLevel;
        private readonly object _lock = new object();
        private NotifyEntry _lastEntry = null;
        private readonly Timer _progressUpdateTimer; 

        public LoggNotifier(ILogger logger, LoggOptions options)
        {
            _logger = logger;
            _logLevel = options.LogLevel;
            _progressUpdateTimer = new Timer(TimeSpan.FromSeconds(15).TotalMilliseconds);
            _progressUpdateTimer.AutoReset = true;
            _progressUpdateTimer.Enabled = true;
            _progressUpdateTimer.Elapsed += progressUpdateTimerOnElapsed;
        }

        /// <summary>
        /// Progress update. This will happen every n seconds (only latest update will be shown)
        /// </summary>
        /// <param name="entry"></param>
        public void Progress(NotifyEntry entry)
        {
            lock (_lock)
            {
                _lastEntry = entry;
            }
        }

        /// <inheritdoc />
        public void Info(NotifyEntry entry)
        {
            if (!shouldLogg(LogLevelEnum.Information))
            {
                return;
            }

            _logger.Information($"{entry.CorrelationId}; {entry.Message}");
        }

        /// <inheritdoc />
        public void Error(NotifyEntry entry)
        {
            if (!shouldLogg(LogLevelEnum.Error))
            {
                return;
            }

            _logger.Error(entry.Exception, $"{entry.CorrelationId}; {entry.Message}");
        }

        /// <inheritdoc />
        public void Warning(NotifyEntry entry)
        {
            if (!shouldLogg(LogLevelEnum.Warning))
            {
                return;
            }

            if (entry.Exception != null)
            {
                _logger.Warning(entry.Exception, $"{entry.CorrelationId}; {entry.Message}");
            }
            else
            {
                _logger.Warning($"{entry.CorrelationId}; {entry.Message}");
            }

        }

        /// <inheritdoc />
        public void Debug(NotifyEntry entry)
        {
            if (!shouldLogg(LogLevelEnum.Debug))
            {
                return;
            }

            if (entry.Exception != null)
            {
                _logger.Verbose(entry.Exception, $"{entry.CorrelationId}; {entry.Message}");
            }
            else
            {
                _logger.Verbose($"{entry.CorrelationId}; {entry.Message}");
            }
        }

        /// <inheritdoc />
        public void Flush()
        {
            progressUpdateTimerOnElapsed(null, null);
        }

        #region Helpers

        private void progressUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                if (_lastEntry != null)
                {
                    _logger.Information($"{_lastEntry.CorrelationId}; {_lastEntry.Message}");
                }

                _lastEntry = null;
            }
        }

        private bool shouldLogg(LogLevelEnum loggLevel) => loggLevel >= _logLevel;

        #endregion Helpers
    }

    public class LoggOptions
    {
        public LogLevelEnum LogLevel { get; }

        public LoggOptions(LogLevelEnum logLevel)
        {
            LogLevel = logLevel;
        }
    }
}

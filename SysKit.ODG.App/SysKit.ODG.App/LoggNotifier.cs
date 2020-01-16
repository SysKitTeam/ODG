using System;
using System.Collections.Concurrent;
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
        private ProgressEntry _lastEntry = null;
        private readonly Timer _progressUpdateTimer; 
        private readonly ConcurrentStack<string> _correlationIds = new ConcurrentStack<string>();

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
        /// <param name="message"></param>
        public void Progress(string message)
        {
            lock (_lock)
            {
                _lastEntry = new ProgressEntry(message, currentCorrelationId);
            }
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            if (!shouldLogg(LogLevelEnum.Information))
            {
                return;
            }

            _logger.Information($"{currentCorrelationId}; {message}");
        }

        /// <inheritdoc />
        public void Error(string message, Exception exception = null)
        {
            if (!shouldLogg(LogLevelEnum.Error))
            {
                return;
            }

            _logger.Error(exception, $"{currentCorrelationId}; {message}");
        }

        /// <inheritdoc />
        public void Warning(string message)
        {
            if (!shouldLogg(LogLevelEnum.Warning))
            {
                return;
            }

            _logger.Warning($"{currentCorrelationId}; {message}");
        }

        /// <inheritdoc />
        public void Debug(string message)
        {
            if (!shouldLogg(LogLevelEnum.Debug))
            {
                return;
            }

            else
            {
                _logger.Verbose($"{currentCorrelationId}; {message}");
            }
        }

        /// <inheritdoc />
        public void Flush()
        {
            progressUpdateTimerOnElapsed(null, null);
        }

        /// <inheritdoc />
        public void BeginContext(string contextId)
        {
            _correlationIds.Push(contextId);
        }

        /// <inheritdoc />
        public void EndContext()
        {
            _correlationIds.TryPop(out string _);
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

        private string currentCorrelationId => _correlationIds.TryPeek(out string correlationId) ? correlationId : null;

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

    class ProgressEntry
    {
        public string CorrelationId { get; }
        public string Message { get; }

        public ProgressEntry(string message, string correlationId)
        {
            CorrelationId = correlationId;
            Message = message;
        }
    }
}

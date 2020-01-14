using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SysKit.ODG.Base.Notifier
{
    /// <summary>
    /// Helper for progress update
    /// </summary>
    public class ProgressUpdater: IDisposable
    {
        private int _totalCount;
        private int _currentCount;
        private readonly Stopwatch _stopwatch;
        private readonly INotifier _notifier;

        public ProgressUpdater(string correlationId, INotifier notifier)
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _notifier = notifier;
            notifier.BeginContext(correlationId);
            notifier.Info("STARTED");
        }

        public void SetTotalCount(int totalCount)
        {
            _totalCount = totalCount;
        }

        /// <summary>
        /// Updates current count and notifies progress
        /// </summary>
        /// <param name="newProcessedCount">Increment of current count</param>
        public void UpdateProgress(int newProcessedCount)
        {
            var count = Interlocked.Add(ref _currentCount, newProcessedCount);
            if (count > _totalCount)
            {
                count = _totalCount;
            }

            _notifier.Progress($"Processed: {count}/{_totalCount}");
        }

        public void Dispose()
        {
            _notifier.Flush();
            _notifier.Info($"FINISHED ({_stopwatch.Elapsed.TotalSeconds}s)");
            _notifier.EndContext();
        }
    }
}

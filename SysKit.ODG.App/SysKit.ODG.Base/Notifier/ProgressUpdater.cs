using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SysKit.ODG.Base.Notifier
{
    /// <summary>
    /// Helper for progress update
    /// </summary>
    public class ProgressUpdater
    {
        private readonly int _totalCount;
        private int _currentCount;
        private readonly string _correlationId;
        private readonly INotifier _notifier;

        public ProgressUpdater(int totalCount, string correlationId, INotifier notifier)
        {
            _totalCount = totalCount;
            _correlationId = correlationId;
            _notifier = notifier;
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

            _notifier.Progress(new NotifyEntry(_correlationId, $"Processed: {count}/{_totalCount}"));
        }
    }
}

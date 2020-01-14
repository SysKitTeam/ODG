using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Notifier
{
    public interface INotifier
    {
        /// <summary>
        /// Progress update
        /// </summary>
        /// <param name="entry"></param>
        void Progress(NotifyEntry entry);

        /// <summary>
        /// Information update
        /// </summary>
        /// <param name="entry"></param>
        void Info(NotifyEntry entry);

        /// <summary>
        /// Error entry
        /// </summary>
        /// <param name="entry"></param>
        void Error(NotifyEntry entry);

        /// <summary>
        /// Warning entry
        /// </summary>
        /// <param name="entry"></param>
        void Warning(NotifyEntry entry);

        /// <summary>
        /// Debug entry
        /// </summary>
        /// <param name="entry"></param>
        void Debug(NotifyEntry entry);

        /// <summary>
        /// Flushes any buffered entries
        /// </summary>
        void Flush();
    }
}

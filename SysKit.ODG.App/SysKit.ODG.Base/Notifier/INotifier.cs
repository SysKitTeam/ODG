using System;

namespace SysKit.ODG.Base.Notifier
{
    public interface INotifier
    {
        /// <summary>
        /// Progress update
        /// </summary>
        /// <param name="message"></param>
        void Progress(string message);

        /// <summary>
        /// Information update
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);

        /// <summary>
        /// Error update
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        void Error(string message, Exception exception = null);

        /// <summary>
        /// Warning update
        /// </summary>
        /// <param name="message"></param>
        void Warning(string message);

        /// <summary>
        /// Debug entry
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);

        /// <summary>
        /// Flushes any buffered entries
        /// </summary>
        void Flush();

        /// <summary>
        /// Starts logging messages for provided context
        /// </summary>
        /// <param name="contextId"></param>
        void BeginContext(string contextId);

        /// <summary>
        /// Ends logging messages for last context
        /// </summary>
        void EndContext();
    }
}

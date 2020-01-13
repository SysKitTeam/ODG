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
        void Notify(NotifyEntry entry);
    }
}

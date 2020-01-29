using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Office365
{
    public class O365CreationResult<TEntry>
    {
        public IEnumerable<TEntry> CreatedEntries { get; }
        public bool HadErrors { get; }

        public O365CreationResult(IEnumerable<TEntry> createdEntries, bool hadErrors)
        {
            CreatedEntries = createdEntries;
            HadErrors = hadErrors;
        }
    }
}

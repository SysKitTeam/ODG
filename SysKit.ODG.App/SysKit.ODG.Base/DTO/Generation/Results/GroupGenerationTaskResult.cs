using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Interfaces.Generation;

namespace SysKit.ODG.Base.DTO.Generation.Results
{
    public class GroupGenerationTaskResult: IGenerationTaskResult
    {
        public bool HadErrors { get; }
        public IEnumerable<GroupEntry> CreatedGroups { get; }

        public GroupGenerationTaskResult(IEnumerable<GroupEntry> createdGroups, bool hadErrors)
        {
            HadErrors = hadErrors;
            CreatedGroups = createdGroups;
        }
    }
}

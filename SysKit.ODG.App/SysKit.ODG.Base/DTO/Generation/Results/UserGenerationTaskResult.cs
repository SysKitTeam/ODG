using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Interfaces.Generation;

namespace SysKit.ODG.Base.DTO.Generation.Results
{
    public class UserGenerationTaskResult: IGenerationTaskResult
    {
        public bool HadErrors { get; }
        public IEnumerable<UserEntry> CreatedUsers { get; }

        public UserGenerationTaskResult(IEnumerable<UserEntry> createdUsers, bool hadErrors)
        {
            HadErrors = hadErrors;
            CreatedUsers = createdUsers;
        }
    }
}

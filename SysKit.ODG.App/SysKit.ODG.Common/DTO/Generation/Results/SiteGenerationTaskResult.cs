using System.Collections;
using System.Collections.Generic;
using SysKit.ODG.Base.Interfaces.Generation;

namespace SysKit.ODG.Base.DTO.Generation.Results
{
    public class SiteGenerationTaskResult: IGenerationTaskResult
    {
        public bool HadErrors { get; }
        public IEnumerable<SiteEntry> CreatedSites { get; }

        public SiteGenerationTaskResult(IEnumerable<SiteEntry> createdSites, bool hadErrors)
        {
            CreatedSites = createdSites;
            HadErrors = hadErrors;
        }
    }
}

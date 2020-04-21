using System.Collections.Generic;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface ISiteDataGeneration
    {
        IEnumerable<SiteEntry> CreateSites(GenerationOptions generationOptions);
    }
}

using System.Collections.Generic;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGroupDataGeneration
    {
        IEnumerable<UnifiedGroupEntry> CreateUnifiedGroups(GenerationOptions generationOptions);
    }
}
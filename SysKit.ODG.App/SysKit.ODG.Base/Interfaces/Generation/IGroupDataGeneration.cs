using System.Collections.Generic;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Office365;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGroupDataGeneration
    {
        IEnumerable<UnifiedGroupEntry> CreateUnifiedGroupsAndTeams(GenerationOptions generationOptions, IUserEntryCollection userEntryCollection);
    }
}
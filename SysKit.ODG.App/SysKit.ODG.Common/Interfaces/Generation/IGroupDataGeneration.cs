using System.Collections.Generic;
using SysKit.ODG.Base.DTO;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Office365;
using SysKit.ODG.Base.XmlCleanupTemplate;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGroupDataGeneration
    {
        IEnumerable<UnifiedGroupEntry> CreateUnifiedGroupsAndTeams(GenerationOptions generationOptions, IUserEntryCollection userEntryCollection);

        IEnumerable<XmlDirectoryElement> CreateDirectoryElements(IEnumerable<GroupEntry> groups);

        IEnumerable<PrivateTeamChannelCreationOptions> CreatePrivateChannels(
            Dictionary<string, List<string>> teamMembershipLookup);

        List<ContentEntry> GenerateDocumentsFolderStructure(int itemsPerSite, IUserEntryCollection userEntryCollection);
    }
}
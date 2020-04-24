using System.Collections.Generic;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.XmlCleanupTemplate;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface ISiteDataGeneration
    {
        IEnumerable<SiteEntry> CreateSites(GenerationOptions generationOptions);

        IEnumerable<XmlDirectoryElement> CreateDirectoryElements(IEnumerable<SiteEntry> sites);
    }
}

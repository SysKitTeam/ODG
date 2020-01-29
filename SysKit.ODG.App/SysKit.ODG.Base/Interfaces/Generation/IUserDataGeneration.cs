using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.XmlCleanupTemplate;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IUserDataGeneration
    {
        IEnumerable<UserEntry> CreateUsers(UserGenerationOptions generationOptions);

        IEnumerable<XmlDirectoryElement> CreateDirectoryElements(IEnumerable<UserEntry> users);
    }
}

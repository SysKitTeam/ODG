using System.Collections.Generic;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Common.Interfaces.SampleData
{
    public interface IManagerGenerationService
    {
        List<ManagerSubordinatePair> GenerateManagerSubordinatePairs(IEnumerable<UserEntry> users);
    }
}
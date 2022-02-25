using System;
using System.Collections.Generic;
using System.Linq;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Common.Interfaces.SampleData;

namespace SysKit.ODG.SampleData
{
    public class ManagerGenerationService : IManagerGenerationService
    {
        public List<ManagerSubordinatePair> GenerateManagerSubordinatePairs(IEnumerable<UserEntry> users)
        {
            var managerPairs = new List<ManagerSubordinatePair>();
            var companyGrouping = users.GroupBy(u => u.CompanyName);

            foreach (var companyGroup in companyGrouping)
            {
                var departmentGrouping = companyGroup.GroupBy(cu => cu.Department);

                foreach (var departmentGroup in departmentGrouping)
                {
                    var hierarchyLookup = departmentGroup.ToLookup(dg => dg.HierarchyLevel);
                    managerPairs.AddRange(getManagersForDepartment(hierarchyLookup));
                }
            }

            return managerPairs;
        }

        private IEnumerable<ManagerSubordinatePair> getManagersForDepartment(ILookup<int, UserEntry> hierarchyLookup)
        {
            var random = new Random();
            var managerPairs = new List<ManagerSubordinatePair>();
            var hierarchyLevel = 2;

            while (hierarchyLookup.Contains(hierarchyLevel))
            {
                var currentLevel = hierarchyLookup[hierarchyLevel];
                var pastLevel = hierarchyLookup[hierarchyLevel - 1].ToArray();
                var pastLevelLength = pastLevel.Length;

                managerPairs.AddRange(currentLevel.Select(userEntry => new ManagerSubordinatePair() { SubordinateGuid = userEntry.Id, ManagerGuid = pastLevel[random.Next(pastLevelLength)].Id, }));

                hierarchyLevel++;
            }

            return managerPairs;
        }
    }
}
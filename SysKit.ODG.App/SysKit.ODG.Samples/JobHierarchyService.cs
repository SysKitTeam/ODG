using System;
using System.Collections.Generic;
using System.Linq;
using SysKit.ODG.Common.Interfaces.SampleData;
using SysKit.ODG.SampleData.Models;

namespace SysKit.ODG.SampleData
{
    public class JobHierarchyService : IJobHierarchyService
    {
        private readonly Dictionary<string, Dictionary<string, HierarchyState>> _hierarchyLookup;
        private readonly Random _random;

        public JobHierarchyService()
        {
            _hierarchyLookup = new Dictionary<string, Dictionary<string, HierarchyState>>();
            _random = new Random();
        }
        public (int hierarchyLevel, string jobTitle) GetHierarchyLevelAndJobTitle(string company, string department)
        {
            EnsureLookupEntry(company, department);
            var state = GetHierarchyState(company, department);

            int level;
            if (state.RemainingPositionsOnLevel > 0)
            {
                level = state.Level;
            }
            else
            {
                IncreaseLevel(company, department);
                level = GetHierarchyState(company, department).Level;
            }

            DecreaseNumberOfPositionsOnCurrentLevel(company, department);

            var jobTitle = getJobTitleFromLevel(level, department);
            return (level, jobTitle);
        }

        private static string getJobTitleFromLevel(int level, string department)
        {
            switch (level)
            {
                case 1:
                    return $"Head of {department}";
                case 2:
                    return $"Lead of {department} Level 1";
                case 3:
                    return $"Lead of {department} Level 2";
                default:
                    return $"Worker in {department} Level {level - 3}";
            }
        }

        private void IncreaseLevel(string company, string department)
        {
            var level = ++_hierarchyLookup[company][department].Level;
            var workersOnLevel = Convert.ToInt32(Math.Pow(5, level - 1));
            _hierarchyLookup[company][department].RemainingPositionsOnLevel = workersOnLevel;
        }

        private void DecreaseNumberOfPositionsOnCurrentLevel(string company, string department)
        {
            _hierarchyLookup[company][department].RemainingPositionsOnLevel--;
        }

        private HierarchyState GetHierarchyState(string company, string department)
        {
            return _hierarchyLookup[company][department];
        }

        private void EnsureLookupEntry(string company, string department)
        {
            if (!_hierarchyLookup.Keys.Contains(company))
            {
                _hierarchyLookup[company] = new Dictionary<string, HierarchyState>();
            }

            if (!_hierarchyLookup[company].Keys.Contains(department))
            {
                _hierarchyLookup[company][department] = new HierarchyState()
                {
                    Level = 1,
                    RemainingPositionsOnLevel = 1
                };
            }
        }
    }
}
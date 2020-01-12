using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Office365
{
    public class CreatedGroupsResult
    {
        private readonly ConcurrentBag<UnifiedGroupEntry> _createdGroups = new ConcurrentBag<UnifiedGroupEntry>();
        /// <summary>
        /// Groups that didn't have current user as owner. We need to add him to owners so we can create a team from group
        /// Key => userid, Value => group where owner was added
        /// </summary>
        private readonly ConcurrentDictionary<string, UnifiedGroupEntry> _groupsWithAddedOwners = new ConcurrentDictionary<string, UnifiedGroupEntry>();

        public CreatedGroupsResult()
        {

        }

        public void AddGroup(UnifiedGroupEntry createdGroup)
        {
            _createdGroups.Add(createdGroup);
        }

        public void AddGroupWhereOwnerWasAdded(string ownerId, UnifiedGroupEntry group)
        {
            _groupsWithAddedOwners.TryAdd(ownerId, group);
        }

        public List<TeamEntry> TeamsToCreate => _createdGroups.Where(g => g.IsTeam).Cast<TeamEntry>().ToList();
        public List<UnifiedGroupEntry> CreatedGroups => _createdGroups.ToList();

        public Dictionary<string, UnifiedGroupEntry> GroupsWithAddedOwners =>
            _groupsWithAddedOwners.ToDictionary(x => x.Key, x => x.Value);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.Utils;

namespace SysKit.ODG.Base.Office365
{
    /// <summary>
    /// Helper class for searching UserEntries returned from Graph API
    /// </summary>
    public class UserEntryCollection : IUserEntryCollection
    {
        private readonly Dictionary<string, UserEntry> _userEntriesLookup;
        private readonly string _tenantDomain;
        private readonly Dictionary<string, List<UserEntry>> _lookUpByDepartment;
        private readonly List<string> _departmentKeys;
        private int _currentDepartmentKeyIndex;
        private int _numberOfDepartmentTeamsCreated;

        public UserEntryCollection(string tenantDomain, IEnumerable<UserEntry> userEntries)
        {
            _tenantDomain = tenantDomain;
            _userEntriesLookup = new Dictionary<string, UserEntry>(StringComparer.OrdinalIgnoreCase);
            _lookUpByDepartment = new Dictionary<string, List<UserEntry>>(StringComparer.OrdinalIgnoreCase);
            _numberOfDepartmentTeamsCreated = 0;

            foreach (var userEntry in userEntries.Where(userEntry => !string.IsNullOrEmpty(userEntry.UserPrincipalName)))
            {
                _userEntriesLookup[userEntry.UserPrincipalName] = userEntry;

                var key = getDepartmentKey(userEntry);
                if (!_lookUpByDepartment.ContainsKey(key))
                {
                    _lookUpByDepartment[key] = new List<UserEntry>();
                }

                _lookUpByDepartment[key].Add(userEntry);
            }

            _departmentKeys = _lookUpByDepartment.Keys.ToList();
            _currentDepartmentKeyIndex = 0;

        }

        private static string getDepartmentKey(UserEntry userEntry)
        {
            return !string.IsNullOrEmpty(userEntry.CompanyName) && !string.IsNullOrEmpty(userEntry.Department) ? $"{userEntry.CompanyName}-{userEntry.Department}" : "default";
        }

        /// <inheritdoc />
        public UserEntry FindMember(MemberEntry member)
        {
            var mailNickname = member.Name.Contains("@") ? member.Name : $"{member.Name}@{_tenantDomain}";
            return _userEntriesLookup.TryGetValue(mailNickname, out UserEntry value) ? value : null;
        }

        /// <inheritdoc />
        public IEnumerable<MemberEntry> GetRandomEntries(int number)
        {
            var values = _userEntriesLookup.Values.ToList();
            foreach (var entryValue in values.GetRandom(number))
            {
                yield return new MemberEntry(entryValue.UserPrincipalName);
            }
        }

        /// <summary>
        /// Returns User AAD Ids, or fails if some user is missing
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public HashSet<string> GetMemberIds(IEnumerable<MemberEntry> members)
        {
            var userIds = new HashSet<string>();

            if (members == null)
            {
                return userIds;
            }

            foreach (var member in members)
            {
                var memberEntry = FindMember(member);

                if (memberEntry == null)
                {
                    throw new MemberNotFoundException(member.Name);
                }

                userIds.Add(memberEntry.Id);
            }

            return userIds;
        }

        /// <inheritdoc />
        public (List<MemberEntry> members, List<MemberEntry> owners) GetMembersAndOwners(bool createDepartmentTeams)
        {
            var department = getNextDepartmentKey();
            if (createDepartmentTeams && !AreAllDepartmentTeamsCreated)
            {
                return getMembersAndOwnersForDepartmentTeam(department);
            }
            var members = _lookUpByDepartment[department].GetRandom(RandomThreadSafeGenerator.Next(3, 20)).ToList();
            var potentialOwners = members.Where(m => m.JobTitle.Contains("Lead") || m.JobTitle.Contains("Head"));
            var desiredNumberOfOwners = getDesiredNumberOfOwners();
            var owners = new List<UserEntry>();
            owners.AddRange(potentialOwners.Take(desiredNumberOfOwners));

            foreach (var member in members)
            {
                if (owners.Count >= desiredNumberOfOwners)
                {
                    break;
                }

                if (!owners.Contains(member))
                {
                    owners.Add(member);
                }
            }

            members.AddRange(getUsersOutsideDepartment(department, RandomThreadSafeGenerator.Next(3, 10)));

            return (
                members.Select(m => new MemberEntry(m.UserPrincipalName)).ToList(),
                owners.Select(o => new MemberEntry(o.UserPrincipalName)).ToList()
                );
        }

        private bool AreAllDepartmentTeamsCreated => _numberOfDepartmentTeamsCreated >= (_departmentKeys.Count - 1); // We don't count the "default" department

        private (List<MemberEntry> members, List<MemberEntry> owners) getMembersAndOwnersForDepartmentTeam(string departmentKey)
        {
            _numberOfDepartmentTeamsCreated++;
            var members = _lookUpByDepartment[departmentKey];
            var owners = new List<UserEntry>();
            owners.AddRange(members.Where(m => m.JobTitle.Contains("Head")));
            owners.AddRange(members.Where(m => m.JobTitle.Contains("Lead") && m.JobTitle.Contains("Level 1")));

            return (
                members.Take(24000).Select(m => new MemberEntry(m.UserPrincipalName)).ToList(),
                owners.Take(5).Select(o => new MemberEntry(o.UserPrincipalName)).ToList()
            );
        }

        private List<UserEntry> getUsersOutsideDepartment(string department, int numberOfUsers)
        {
            var values = _userEntriesLookup.Values.ToList();
            var usersOutsideDepartment = new List<UserEntry>();

            while (usersOutsideDepartment.Count < numberOfUsers)
            {
                var user = values[RandomThreadSafeGenerator.Next(values.Count)];
                if (string.Compare(getDepartmentKey(user), department, StringComparison.OrdinalIgnoreCase) != 0 && !usersOutsideDepartment.Contains(user))
                {
                    usersOutsideDepartment.Add(user);
                }
            }

            return usersOutsideDepartment;
        }

        private static int getDesiredNumberOfOwners()
        {
            var percentile = RandomThreadSafeGenerator.Next(100);
            switch (percentile)
            {
                case 1:
                    return 1;
                case 2:
                    return RandomThreadSafeGenerator.Next(6, 10);
                default:
                    return RandomThreadSafeGenerator.Next(2, 5);
            }
        }

        private string getNextDepartmentKey()
        {
            var departmentKey = _departmentKeys[_currentDepartmentKeyIndex];

            //Skip default department
            if (departmentKey == "default")
            {
                _currentDepartmentKeyIndex = (_currentDepartmentKeyIndex + 1) % _departmentKeys.Count;
                departmentKey = _departmentKeys[_currentDepartmentKeyIndex];
            }

            _currentDepartmentKeyIndex = (_currentDepartmentKeyIndex + 1) % _departmentKeys.Count;

            return departmentKey;
        }
    }
}

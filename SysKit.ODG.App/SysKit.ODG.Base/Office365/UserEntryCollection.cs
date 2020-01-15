using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.Utils;
using SysKit.ODG.Base.XmlTemplate.Model;

namespace SysKit.ODG.Base.Office365
{
    /// <summary>
    /// Helper class for searching UserEntries returned from Graph API
    /// </summary>
    public class UserEntryCollection : IUserEntryCollection
    {
        private readonly Dictionary<string, UserEntry> _userEntriesLookup;
        private readonly string _tenantDomain;

        public UserEntryCollection(string tenantDomain, IEnumerable<UserEntry> userEntries)
        {
            _tenantDomain = tenantDomain;
            _userEntriesLookup = new Dictionary<string, UserEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var userEntry in userEntries)
            {
                if (string.IsNullOrEmpty(userEntry.UserPrincipalName))
                {
                    continue;
                }

                _userEntriesLookup[userEntry.UserPrincipalName] = userEntry;
            }
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
    }
}

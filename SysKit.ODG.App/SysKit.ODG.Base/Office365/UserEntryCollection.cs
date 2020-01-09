using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.XmlTemplate.Model;

namespace SysKit.ODG.Base.Office365
{
    /// <summary>
    /// Helper class for searching UserEntries returned from Graph API
    /// </summary>
    public class UserEntryCollection
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

        /// <summary>
        /// Returns UserEntry or null if member is not found
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public UserEntry FindMember(MemberEntry member)
        {
            var mailNickname = member.Name.Contains("@") ? member.Name : $"{member.Name}@{_tenantDomain}";
            return _userEntriesLookup.TryGetValue(mailNickname, out UserEntry value) ? value : null;
        }
    }
}

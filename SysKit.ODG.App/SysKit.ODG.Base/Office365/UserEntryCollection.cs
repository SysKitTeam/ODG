using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;
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
            var usedEntries = new HashSet<int>();
            var usedEntryValues = _userEntriesLookup.Values.ToList();
            var maxValue = _userEntriesLookup.Count;
            var numberOfEntries = Math.Min(number, maxValue);
            int counter = 0;

            while (counter < numberOfEntries)
            {
                int randomIndex;
                do
                {
                    randomIndex = RandomThreadSafeGenerator.Next(maxValue);
                } while (usedEntries.Contains(randomIndex));

                usedEntries.Add(randomIndex);
                counter++;
                yield return new MemberEntry(usedEntryValues[randomIndex].UserPrincipalName);
            }
        }
    }
}

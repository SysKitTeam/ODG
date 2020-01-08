using System;
using System.Collections.Generic;
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
        public UserEntryCollection(IEnumerable<UserEntry> userEntries)
        {
            _userEntriesLookup = userEntries.ToDictionary(entry => entry.MailNickname, entry => entry);
        }

        /// <summary>
        /// Returns UserEntry or null if member is not found
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public UserEntry FindMember(XmlMember member)
        {
            var mailNickname = member.Name.Contains("@") ? member.Name.Split('@')[0] : member.Name;
            return _userEntriesLookup.TryGetValue(mailNickname, out UserEntry value) ? value : null;
        }
    }
}

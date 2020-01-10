using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class TeamChannelEntry
    {
        public string DisplayName { get; set; }
        public bool IsPrivate { get; set; }

        public List<MemberEntry> Owners { get; set; }
        public List<MemberEntry> Members { get; set; }

        public TeamChannelEntry(string displayName, bool isPrivate)
        {
            DisplayName = displayName;
            IsPrivate = isPrivate;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class GroupEntry
    {
        public string DisplayName { get; set; }

        public List<MemberEntry> Owners { get; set; }
        public List<MemberEntry> Members { get; set; }

    }
}

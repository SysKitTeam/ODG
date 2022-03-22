using System.Collections.Generic;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class GroupEntry
    {
        public string GroupId { get; set; }
        public string DisplayName { get; set; }

        public List<MemberEntry> Owners { get; set; }
        public List<MemberEntry> Members { get; set; }
        public string Template { get; set; }

    }
}

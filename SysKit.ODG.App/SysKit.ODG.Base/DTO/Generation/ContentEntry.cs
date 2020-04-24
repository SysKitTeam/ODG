using System.Collections.Generic;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class ContentEntry: IRoleAssignments
    {
        public string Name { get; }
        public ContentTypeEnum Type { get; }
        public List<ContentEntry> Children { get; set; }

        #region IRoleAssignments

        public bool HasUniqueRoleAssignments { get; set; }
        public bool CopyFromParent { get; set; }
        public Dictionary<RoleTypeEnum, HashSet<MemberEntry>> Assignments { get; set; }
        public List<SharingLinkEntry> SharingLinks { get; set; }

        #endregion IRoleAssignements

        public ContentEntry(string name, ContentTypeEnum type)
        {
            Name = name;
            Type = type;
            Children = new List<ContentEntry>();
            Assignments = new Dictionary<RoleTypeEnum, HashSet<MemberEntry>>();
            SharingLinks = new List<SharingLinkEntry>();
        }
    }
}

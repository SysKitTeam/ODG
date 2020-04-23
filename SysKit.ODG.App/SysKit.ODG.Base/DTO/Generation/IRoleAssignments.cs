using System.Collections.Generic;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.DTO.Generation
{
    public interface IRoleAssignments
    {
        bool HasUniqueRoleAssignments { get; set; }
        bool CopyFromParent { get; set; }
        Dictionary<RoleTypeEnum, HashSet<MemberEntry>> Assignments { get; set; }
    }
}

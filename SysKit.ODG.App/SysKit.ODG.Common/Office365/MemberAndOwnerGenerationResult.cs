using System.Collections.Generic;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Office365
{
    public class MemberAndOwnerGenerationResult
    {
        public List<MemberEntry> Members { get; set; }
        public List<MemberEntry> Owners { get; set; }
        public bool IsDepartmentTeam { get; set; }
        public string DepartmentTeamName { get; set; }
        public string Template { get; set; }
    }
}
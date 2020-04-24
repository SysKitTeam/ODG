using System.Collections.Generic;

namespace SysKit.ODG.Base.DTO.Generation
{
    public interface IAssociatedSPGroups
    {
        string Url { get; }
        List<MemberEntry> SPOwners { get; set; }
        List<MemberEntry> SPMembers { get; set; }
        List<MemberEntry> SPVisitors { get; set; }
    }
}

using System.Collections.Generic;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class SiteEntry: ISharePointContent, IAssociatedSPGroups
    {
        public string Title { get; set; }
        public string Url { get; set; }

        public MemberEntry Owner { get; set; }
        public List<MemberEntry> SiteAdmins { get; set; }

        #region Associated SharePoint Groups

        public List<MemberEntry> SPOwners { get; set; }
        public List<MemberEntry> SPMembers { get; set; }
        public List<MemberEntry> SPVisitors { get; set; }

        #endregion Associated SharePoint Groups

        public ContentEntry Content { get; set; }
    }
}

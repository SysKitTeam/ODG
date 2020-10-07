using System;
using System.Collections.Generic;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class UnifiedGroupEntry : GroupEntry, ISharePointContent, IAssociatedSPGroups
    {
        public string MailNickname { get; set; }
        public string SiteUrl { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsTeam { get; protected set; }

        public bool ProvisionFailed => string.IsNullOrEmpty(SiteUrl);

        #region Associated SharePoint Groups

        public List<MemberEntry> SPOwners { get; set; }
        public List<MemberEntry> SPMembers { get; set; }
        public List<MemberEntry> SPVisitors { get; set; }

        #endregion Associated SharePoint Groups

        public string Url => SiteUrl;
        public ContentEntry Content { get; set; }
        public Guid? SiteGuid { get; set; }
    }
}

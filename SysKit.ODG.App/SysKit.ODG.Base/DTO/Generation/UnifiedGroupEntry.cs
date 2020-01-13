using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class UnifiedGroupEntry: GroupEntry
    {
        public string GroupId { get; set; }
        public string MailNickname { get; set; }
        public string SiteUrl { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsTeam { get; protected set; }
    }
}

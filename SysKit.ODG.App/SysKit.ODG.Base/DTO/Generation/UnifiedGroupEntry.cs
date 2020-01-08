using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class UnifiedGroupEntry: GroupEntry
    {
        public string Mail { get; set; }
        public string MailNickname { get; set; }
        public string SiteUrl { get; set; }
        public string Classification { get; set; }
        public string Visibility { get; set; }
    }
}

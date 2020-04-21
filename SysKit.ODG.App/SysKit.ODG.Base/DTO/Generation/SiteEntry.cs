using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class SiteEntry
    {
        public string Title { get; set; }
        public string Url { get; set; }

        public MemberEntry Owner { get; set; }
        public List<MemberEntry> SiteAdmins { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class MemberEntry
    {
        public string Name { get; set; }

        public MemberEntry(string name)
        {
            Name = name;
        }
    }
}

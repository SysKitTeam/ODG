using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model
{
    [XmlType(TypeName = "RandomOptions")]
    public class XmlRandomOptions
    {
        [XmlElement]
        public int NumberOfUsers { get; set; }

        [XmlElement]
        public int NumberOfUnifiedGroups { get; set; }

        [XmlElement]
        public int MaxNumberOfOwnersPerGroup { get; set; }

        [XmlElement]
        public int MaxNumberOfMembersPerGroup { get; set; }
    }
}

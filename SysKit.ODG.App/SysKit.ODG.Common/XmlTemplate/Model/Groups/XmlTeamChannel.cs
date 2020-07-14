using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model.Groups
{
    [XmlType(TypeName = "Channel")]
    public class XmlTeamChannel
    {
        [XmlAttribute]
        public string DisplayName { get; set; }

        [XmlAttribute]
        public bool IsPrivate { get; set; }

        [XmlArray("Owners")]
        public XmlMember[] Owners { get; set; }

        [XmlArray("Members")]
        public XmlMember[] Members { get; set; }
    }
}

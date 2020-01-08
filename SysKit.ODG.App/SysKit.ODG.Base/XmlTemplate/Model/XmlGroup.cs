using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model
{
    [XmlType(TypeName = "Group")]
    [XmlInclude(typeof(XmlUnifiedGroup))]
    public class XmlGroup
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string DisplayName { get; set; }

        [XmlArray("Owners")]
        public XmlMember[] Owners { get; set; }

        [XmlArray("Members")]
        public XmlMember[] Members { get; set; }
    }
}

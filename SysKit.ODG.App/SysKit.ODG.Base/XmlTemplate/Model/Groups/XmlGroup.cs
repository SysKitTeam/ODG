using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model.Groups
{
    [XmlType(TypeName = "Group")]
    [XmlInclude(typeof(XmlUnifiedGroup))]
    [XmlInclude(typeof(XmlTeam))]
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

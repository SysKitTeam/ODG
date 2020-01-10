using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model.Groups
{
    [XmlType(TypeName = "Team")]
    public class XmlTeam: XmlUnifiedGroup
    {
        [XmlArray("Channels")]
        public XmlTeamChannel[] Channels { get; set; }
    }
}

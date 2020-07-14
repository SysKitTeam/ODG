using System.Xml.Serialization;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.XmlTemplate.Model.Content
{
    [XmlType("RoleAssignment")]
    public class XmlRoleAssignment
    {
        [XmlAttribute]
        public RoleTypeEnum Role { get; set; }

        [XmlArray]
        public XmlMember[] Members { get; set; }
    }
}

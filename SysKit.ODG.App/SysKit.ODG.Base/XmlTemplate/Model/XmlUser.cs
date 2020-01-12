using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model
{
    [XmlType(TypeName = "User")]
    public class XmlUser
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public bool AccountDisabled { get; set; }
    }
}

using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model.Groups
{
    [XmlType(TypeName = "UnifiedGroup")]
    public class XmlUnifiedGroup: XmlGroup
    {
        [XmlAttribute]
        public bool IsPrivate { get; set; }
    }
}

using System.Xml.Serialization;
using SysKit.ODG.Base.XmlTemplate.Model.Content;

namespace SysKit.ODG.Base.XmlTemplate.Model.Groups
{
    [XmlType(TypeName = "UnifiedGroup")]
    public class XmlUnifiedGroup: XmlGroup
    {
        [XmlAttribute]
        public bool IsPrivate { get; set; }

        [XmlArray(IsNullable = true)]
        public XmlWeb[] SharePointContent { get; set; }
    }
}

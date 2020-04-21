using System.Xml.Serialization;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.XmlTemplate.Model.Content
{
    [XmlType("Web")]
    public class XmlWeb: XmlContent
    {
        [XmlAttribute]
        public bool IsRootWeb { get; set; }
        public override ContentTypeEnum Type => ContentTypeEnum.Web;
    }
}

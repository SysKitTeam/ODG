using System.Xml.Serialization;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.XmlTemplate.Model.Content
{
    [XmlType("Web")]
    public class XmlWeb: XmlContent
    {
        public override ContentTypeEnum Type => ContentTypeEnum.Web;
    }
}

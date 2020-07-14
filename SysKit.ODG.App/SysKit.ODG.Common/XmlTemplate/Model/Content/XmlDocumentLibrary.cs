using System.Xml.Serialization;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.XmlTemplate.Model.Content
{
    [XmlType("DocumentLibrary")]
    public class XmlDocumentLibrary: XmlContent
    {
        public override ContentTypeEnum Type => ContentTypeEnum.DocumentLibrary;
    }
}

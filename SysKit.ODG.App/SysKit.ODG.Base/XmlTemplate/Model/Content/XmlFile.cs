using System.Xml.Serialization;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.XmlTemplate.Model.Content
{
    [XmlType("File")]
    public class XmlFile : XmlListItemContent
    {
        public override ContentTypeEnum Type => ContentTypeEnum.File;
    }
}

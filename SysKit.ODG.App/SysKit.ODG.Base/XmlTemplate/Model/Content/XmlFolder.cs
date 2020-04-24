using System.Xml.Serialization;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.XmlTemplate.Model.Content
{
    [XmlType("Folder")]
    public class XmlFolder : XmlListItemContent
    {
        public override ContentTypeEnum Type => ContentTypeEnum.Folder;
    }
}

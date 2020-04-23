using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model.Content
{
    [XmlType("SharingLink")]
    public class XmlSharingLink
    {
        [XmlAttribute]
        public bool IsEdit { get; set; }
    }
}

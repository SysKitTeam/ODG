using System.Xml.Serialization;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.XmlCleanupTemplate
{
    [XmlType(TypeName = "DirectoryElement")]
    public class XmlDirectoryElement
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Url { get; set; }

        [XmlAttribute]
        public string DisplayName { get; set; }

        [XmlAttribute]
        public DirectoryElementTypeEnum Type { get; set; }
    }
}

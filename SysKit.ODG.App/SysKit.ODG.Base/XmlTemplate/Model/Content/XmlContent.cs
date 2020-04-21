using System.Xml.Serialization;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.XmlTemplate.Model.Content
{
    [XmlType(TypeName = "Content")]
    [XmlInclude(typeof(XmlWeb))]
    [XmlInclude(typeof(XmlDocumentLibrary))]
    [XmlInclude(typeof(XmlFolder))]
    [XmlInclude(typeof(XmlFile))]
    public class XmlContent
    {
        [XmlAttribute]
        public string Name { get; set; }

        public virtual ContentTypeEnum Type { get; }

        [XmlArrayItem(typeof(XmlWeb))]
        [XmlArrayItem(typeof(XmlDocumentLibrary))]
        [XmlArrayItem(typeof(XmlFolder))]
        [XmlArrayItem(typeof(XmlFile))]
        public XmlContent[] Children { get; set; }
    }
}

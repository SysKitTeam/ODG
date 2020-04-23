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

        /// <summary>
        /// Sets if item has unique role assignments
        /// </summary>
        [XmlAttribute]
        public bool IsUniqueRa { get; set; }
        /// <summary>
        /// Sets if permissions will be copied from parent when breaking them (IsUniqueRA must be true)
        /// </summary>
        [XmlAttribute]
        public bool CopyFromParent { get; set; }
        /// <summary>
        /// Unique role assignments for item
        /// </summary>
        [XmlArray]
        public XmlRoleAssignment[] RoleAssignments { get; set; }
    }
}

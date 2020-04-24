using System.Xml.Serialization;
using SysKit.ODG.Base.XmlTemplate.Model.Content;

namespace SysKit.ODG.Base.XmlTemplate.Model.Groups
{
    [XmlType(TypeName = "UnifiedGroup")]
    public class XmlUnifiedGroup: XmlGroup
    {
        [XmlAttribute]
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Members of SharePoint Owners Group
        /// </summary>
        [XmlArray("SPOwners")]
        public XmlMember[] SPOwners { get; set; }
        /// <summary>
        /// Members of SharePoint Members Group
        /// </summary>
        [XmlArray("SPMembers")]
        public XmlMember[] SPMembers { get; set; }
        /// <summary>
        /// Members of SharePoint Visitors Group
        /// </summary>
        [XmlArray("SPVisitors")]
        public XmlMember[] SPVisitors { get; set; }

        [XmlElement("SharePointContent")]
        public XmlWeb SharePointContent { get; set; }
    }
}

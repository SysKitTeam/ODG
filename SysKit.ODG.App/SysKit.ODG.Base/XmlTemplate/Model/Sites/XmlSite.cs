using System.Xml.Serialization;
using SysKit.ODG.Base.XmlTemplate.Model.Content;

namespace SysKit.ODG.Base.XmlTemplate.Model.Sites
{
    [XmlType(TypeName = "Site")]
    public class XmlSite
    {
        [XmlElement("PrimaryAdmin")]
        public XmlMember PrimaryAdmin { get; set; }
        [XmlArray("SiteAdmins")]
        public XmlMember[] SiteAdmins { get; set; }


        /// <summary>
        /// Members of SharePoint Owners Group
        /// </summary>
        [XmlArray("Owners")]
        public XmlMember[] SPOwners { get; set; }
        /// <summary>
        /// Members of SharePoint Members Group
        /// </summary>
        [XmlArray("Members")]
        public XmlMember[] SPMembers { get; set; }
        /// <summary>
        /// Members of SharePoint Visitors Group
        /// </summary>
        [XmlArray("Visitors")]
        public XmlMember[] SPVisitors { get; set; }

        [XmlAttribute]
        public string Title { get; set; }

        [XmlAttribute]
        public string RelativeUrl { get; set; }

        [XmlElement("SharePointContent")]
        public XmlWeb SharePointContent { get; set; }
    }
}

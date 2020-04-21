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

        [XmlAttribute]
        public string Title { get; set; }

        [XmlAttribute]
        public string RelativeUrl { get; set; }

        [XmlArray("SharePointContent")]
        public XmlWeb[] SharePointContent { get; set; }
    }
}

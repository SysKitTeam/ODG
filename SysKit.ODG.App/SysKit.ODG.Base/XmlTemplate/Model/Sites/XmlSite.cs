using System.Xml.Serialization;

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
    }
}

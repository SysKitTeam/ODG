using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model
{
    [XmlRoot("Users")]
    public class XmlUserCollection
    {
        [XmlElement()]
        public XmlUser[] Users { get; set; }
    }
}

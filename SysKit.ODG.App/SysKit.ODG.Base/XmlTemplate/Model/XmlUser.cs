using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model
{
    public class XmlUser
    {
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public bool? AccountEnabled { get; set; }
        [XmlAttribute]
        public bool? SetUserPhoto { get; set; }
    }
}

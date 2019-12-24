using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace SysKit.ODG.XMLSpecification.Model
{
    public class XmlUser
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public bool? AccountEnabled { get; set; }
        [XmlAttribute]
        public bool? SetUserPhoto { get; set; }
    }
}

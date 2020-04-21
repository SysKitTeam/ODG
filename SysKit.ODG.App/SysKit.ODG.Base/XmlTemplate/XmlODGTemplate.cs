using System.Xml.Serialization;
using SysKit.ODG.Base.XmlTemplate.Model;
using SysKit.ODG.Base.XmlTemplate.Model.Groups;
using SysKit.ODG.Base.XmlTemplate.Model.Sites;

namespace SysKit.ODG.Base.XmlTemplate
{
    [XmlRoot("ODGTemplate")]
    public class XmlODGTemplate
    {
        [XmlElement(IsNullable = true)]
        public XmlRandomOptions RandomOptions { get; set; }

        [XmlArray(IsNullable = true)]
        public XmlUser[] Users { get; set; }

        [XmlArray(IsNullable = true)]
        public XmlGroup[] Groups { get; set; }

        [XmlArray(IsNullable = true)]
        public XmlSite[] Sites { get; set; }
    }
}

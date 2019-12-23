using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SysKit.ODG.XMLSpecification.Model
{
    [XmlRoot("Users")]
    public class XmlUserCollection
    {
        [XmlElement(IsNullable = false)]
        public XmlUser[] Users { get; set; }
    }
}

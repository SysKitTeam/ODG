using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SysKit.ODG.XMLSpecification.Model
{
    [XmlRoot("ODGSpec")]
    public class XmlODGSpecification
    {
        [XmlElement(IsNullable = true)]
        public XmlUserCollection UserCollection { get; set; }
    }
}

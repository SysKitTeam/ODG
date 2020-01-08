using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model
{
    public class XmlUserRandomOptions
    {
        [XmlElement()]
        public int NumberOfUsers { get; set; }
        [XmlElement(IsNullable = true)]
        public int? NumberOfExternalUsers { get; set; }
    }
}

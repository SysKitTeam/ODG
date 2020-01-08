using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using SysKit.ODG.Base.XmlTemplate.Model;

namespace SysKit.ODG.Base.XmlTemplate
{
    [XmlRoot("ODGTemplate")]
    public class XmlODGTemplate
    {
        [XmlElement(IsNullable = true)]
        public XmlUserCollection UserCollection { get; set; }
    }
}

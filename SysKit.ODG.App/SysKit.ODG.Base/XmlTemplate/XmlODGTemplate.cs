using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using SysKit.ODG.Base.XmlTemplate.Model;
using SysKit.ODG.Base.XmlTemplate.Model.Groups;

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
    }
}

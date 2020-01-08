using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.XmlTemplate.Model
{
    [XmlType(TypeName = "Member")]
    public class XmlMember
    {
        [XmlAttribute]
        public string Name { get; set; }
    }
}

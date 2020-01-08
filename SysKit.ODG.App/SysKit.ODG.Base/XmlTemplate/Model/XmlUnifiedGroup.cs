using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model
{
    [XmlType(TypeName = "UnifiedGroup")]
    public class XmlUnifiedGroup: XmlGroup
    {
        [XmlAttribute]
        public bool IsPrivate { get; set; }
    }
}

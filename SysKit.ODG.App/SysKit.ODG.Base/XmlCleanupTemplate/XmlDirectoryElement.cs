using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.XmlCleanupTemplate
{
    [XmlType(TypeName = "DirectoryElement")]
    public class XmlDirectoryElement
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public DirectoryElementTypeEnum Type { get; set; }
    }
}

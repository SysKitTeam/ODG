using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlCleanupTemplate
{
    [XmlRoot("ODGCleanupTemplate")]
    public class XmlODGCleanupTemplate
    {
        [XmlArray(IsNullable = true)]
        public XmlDirectoryElement[] DirectoryElements { get; set; }
    }
}

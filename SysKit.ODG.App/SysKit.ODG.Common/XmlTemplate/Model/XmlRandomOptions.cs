using System.Xml;
using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model
{
    [XmlType(TypeName = "RandomOptions")]
    public class XmlRandomOptions
    {
        [XmlElement]
        public int NumberOfUsers { get; set; }

        [XmlElement]
        public int NumberOfUnifiedGroups { get; set; }

        [XmlElement]
        public int NumberOfTeams { get; set; }

        [XmlElement]
        public bool CreateDepartmentTeams { get; set; }
    }
}

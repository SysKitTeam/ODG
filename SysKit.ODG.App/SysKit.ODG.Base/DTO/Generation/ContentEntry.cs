using System.Collections.Generic;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class ContentEntry
    {
        public string Name { get; }
        public ContentTypeEnum Type { get; }

        public List<ContentEntry> Children { get; set; }

        public ContentEntry(string name, ContentTypeEnum type)
        {
            Name = name;
            Type = type;
            Children = new List<ContentEntry>();
        }
    }
}

using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Common.DTO.Generation
{
    public class FileEntry : ContentEntry
    {
        public string Extension { get; }
        public string NameWithExtension => Name + Extension;

        public FileEntry(string name, string extension) : base(name, ContentTypeEnum.File)
        {
            Extension = extension;
        }
    }
}
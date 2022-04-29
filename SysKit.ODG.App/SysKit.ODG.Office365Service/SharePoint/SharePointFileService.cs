using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SysKit.ODG.Base.Interfaces.Office365Service;

namespace SysKit.ODG.Office365Service.SharePoint
{
    public class SharePointFileService : ISharePointFileService
    {
        private static readonly ReadOnlyDictionary<string, string> _extensionFileNamesLookup =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
            {
                {".xlsx","Book.xlsx"},
                {".docx","Document.docx"},
                {".vsdx","Drawing.vsdx"},
                {".pptx","Presentation.pptx"},
                {".txt","Text.txt"}
            });

        public ReadOnlyDictionary<string, string> GetExtensionFileNamesLookup()
        {
            return _extensionFileNamesLookup;
        }

        private static List<string> _fileExtensions;

        public List<string> GetFileExtensions()
        {
            if (_fileExtensions == null)
            {
                _fileExtensions = _extensionFileNamesLookup.Keys.ToList();
            }

            return _fileExtensions;
        }
    }
}
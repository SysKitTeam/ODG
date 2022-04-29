using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface ISharePointFileService
    {
        /// <summary>
        /// Returns a list of file extensions that can be created
        /// </summary>
        /// <returns></returns>
        List<string> GetFileExtensions();

        /// <summary>
        /// Get file names for registered extensions
        /// </summary>
        /// <returns></returns>
        ReadOnlyDictionary<string, string> GetExtensionFileNamesLookup();
    }
}
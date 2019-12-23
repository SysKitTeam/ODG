using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Authentication;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGenerationOptions
    {
        SimpleUserCredentials UserCredentials { get; }
        /// <summary>
        /// Filepath to XML specification
        /// </summary>
        string TemplateFilePath { get; set; }
        /// <summary>
        /// If set this password will be used as default password for all new users
        /// </summary>
        string DefaultPassword { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;

namespace SysKit.ODG.Base.DTO
{
    public class GenerationOptionsDTO: IGenerationOptions
    {
        public SimpleUserCredentials UserCredentials { get; }
        public string TemplateFilePath { get; set; }
        public string DefaultPassword { get; set; }

        public GenerationOptionsDTO(SimpleUserCredentials userCredentials)
        {
            UserCredentials = userCredentials;
        }
    }
}

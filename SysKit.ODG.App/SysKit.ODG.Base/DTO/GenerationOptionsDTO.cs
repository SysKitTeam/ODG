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

        public GenerationOptionsDTO(SimpleUserCredentials userCredentials)
        {
            UserCredentials = userCredentials;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGenerationService
    {
        void Start(IGenerationOptions generationOptions);
    }
}

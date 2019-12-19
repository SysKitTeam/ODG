using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGenerationService
    {
        void Start(IGenerationOptions generationOptions);
    }
}

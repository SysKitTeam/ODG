using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.DTO.Generation.Results;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGenerationCleanupService
    {
        void SaveCleanupTemplate(GenerationResult result, string filePath);
    }
}

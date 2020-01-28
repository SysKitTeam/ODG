using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysKit.ODG.Base.Interfaces.Generation;

namespace SysKit.ODG.Base.DTO.Generation.Results
{
    public class GenerationResult
    {
        public readonly Dictionary<string, IGenerationTaskResult> GenerationTaskResults;

        public GenerationResult(Dictionary<string, IGenerationTaskResult> generationTaskResult)
        {
            GenerationTaskResults = generationTaskResult;
        }

        public bool HasErrors => GenerationTaskResults.Values.Any(v => v.HadErrors);
    }
}

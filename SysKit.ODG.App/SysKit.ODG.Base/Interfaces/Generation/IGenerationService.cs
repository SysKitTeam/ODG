using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGenerationService
    {
        /// <summary>
        /// Add tasks that will be execute on Start
        /// </summary>
        /// <param name="task"></param>
        void AddGenerationTask(IGenerationTask task);
        /// <summary>
        /// Execute added tasks
        /// </summary>
        /// <param name="generationOptions"></param>
        void Start(GenerationOptions generationOptions);
    }
}

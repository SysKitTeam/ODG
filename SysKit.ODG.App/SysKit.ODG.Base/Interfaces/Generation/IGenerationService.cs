using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGenerationService
    {
        /// <summary>
        /// Add tasks that will be execute on Start
        /// </summary>
        /// <param name="taskKey">Unique task key. Used for logging</param>
        /// <param name="task"></param>
        void AddGenerationTask(string taskKey, IGenerationTask task);
        /// <summary>
        /// Execute added tasks
        /// </summary>
        /// <param name="generationOptions"></param>
        Task Start(GenerationOptions generationOptions);
    }
}

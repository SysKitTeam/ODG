using SysKit.ODG.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.DTO.Generation.Results;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Generation
{
    public class GenerationService: IGenerationService
    {
        private readonly Dictionary<string, IGenerationTask> _generationTasks = new Dictionary<string, IGenerationTask>();

        public GenerationService()
        {
        }

        public void AddGenerationTask(string taskKey, IGenerationTask task)
        {
            _generationTasks.Add(taskKey, task);
        }

        public async Task<GenerationResult> Start(GenerationOptions generationOptions, INotifier notifier)
        {
            var result = new Dictionary<string, IGenerationTaskResult>();
            foreach (var task in _generationTasks)
            {
                using (new ProgressUpdater(task.Key, notifier))
                {
                    result.Add(task.Key, await task.Value.Execute(generationOptions, notifier));
                }
            }

            return new GenerationResult(result);
        }
    }
}

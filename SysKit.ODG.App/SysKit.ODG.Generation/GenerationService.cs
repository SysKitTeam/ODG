using SysKit.ODG.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
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

        public async Task Start(GenerationOptions generationOptions, INotifier notifier)
        {
            foreach (var task in _generationTasks)
            {
                try
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    notifier.Info(new NotifyEntry($"{task.Key}", $"Started executing"));
                    await task.Value.Execute(generationOptions, notifier);
                    stopwatch.Stop();
                    notifier.Flush();
                    notifier.Info(new NotifyEntry($"{task.Key}", $"Finished executing, Duration: {stopwatch.Elapsed}"));
                }
                catch (Exception e)
                {
                    notifier.Error(new NotifyEntry($"{task.Key}", e));
                    throw;
                }
            }
        }
    }
}

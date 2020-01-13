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

namespace SysKit.ODG.Generation
{
    public class GenerationService: IGenerationService
    {
        private readonly Dictionary<string, IGenerationTask> _generationTasks = new Dictionary<string, IGenerationTask>();
        private readonly ILogger _logger;

        public GenerationService(ILogger logger)
        {
            _logger = logger;
        }

        public void AddGenerationTask(string taskKey, IGenerationTask task)
        {
            _generationTasks.Add(taskKey, task);
        }

        public async Task Start(GenerationOptions generationOptions)
        {
            foreach (var task in _generationTasks)
            {
                try
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    _logger.Information($"Started executing: {task.Key}");
                    await task.Value.Execute(generationOptions, null);
                    stopwatch.Stop();
                    _logger.Information($"Finished executing: {task.Key}, Duration: {stopwatch.Elapsed}");
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Task {task.Key} failed with error");
                    throw;
                }
            }
        }
    }
}

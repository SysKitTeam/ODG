using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Notifier;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGenerationTask
    {
        Task<IGenerationTaskResult> Execute(GenerationOptions options, INotifier notifier);
    }
}

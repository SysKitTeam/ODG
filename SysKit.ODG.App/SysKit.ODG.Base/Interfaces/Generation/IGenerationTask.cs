using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGenerationTask
    {
        Task Execute(GenerationOptions options);
    }
}

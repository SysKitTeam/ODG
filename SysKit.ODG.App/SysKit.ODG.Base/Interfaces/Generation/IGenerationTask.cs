using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGenerationTask
    {
        Task Execute(IGenerationOptions options);
    }
}

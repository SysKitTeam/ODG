using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IUserDataGeneration
    {
        // TODO: UserGenerationOptions!!!
        IEnumerable<UserEntry> CreateUsers(GenerationOptions generationOptions);
    }
}

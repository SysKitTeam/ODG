using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IUserDataGeneration
    {
        IEnumerable<UserEntry> CreateUsers(UserGenerationOptions generationOptions);
    }
}

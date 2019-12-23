using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IDataGenerationFactory
    {
        IEnumerable<UserEntry> GetUserData(IGenerationOptions options);
    }
}

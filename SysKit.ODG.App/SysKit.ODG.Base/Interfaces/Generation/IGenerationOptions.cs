using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Authentication;

namespace SysKit.ODG.Base.Interfaces.Generation
{
    public interface IGenerationOptions
    {
        SimpleUserCredentials UserCredentials { get; }
    }
}

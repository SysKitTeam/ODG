using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;

namespace SysKit.ODG.Base.Options
{
    public class RandomGenerationOptions: IGenerationOptions
    {
        public SimpleUserCredentials UserCredentials { get; }
        public string TenantDomain { get; }
        public string DefaultPassword { get; set; }

        public UserGenerationOptions UserOptions { get; set; }

        public RandomGenerationOptions(SimpleUserCredentials userCredentials, string tenantDomain)
        {
            UserCredentials = userCredentials;
            TenantDomain = tenantDomain;
            UserOptions = new UserGenerationOptions();
        }
    }
}

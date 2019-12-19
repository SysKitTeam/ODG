using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface IUserGraphApiClient
    {
        void GetAllTenantUsers();
    }
}

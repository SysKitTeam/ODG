using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface IUserGraphApiClient
    {
        void GetAllTenantUsers();
        Task CreateTenantUsers(IEnumerable<UserEntry> users);
    }
}

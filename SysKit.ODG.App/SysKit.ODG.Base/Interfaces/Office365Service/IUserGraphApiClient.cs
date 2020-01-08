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
        /// <summary>
        /// Creates users on tenant and returns successfully created users
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        Task<List<UserEntry>> CreateTenantUsers(IEnumerable<UserEntry> users);
    }
}

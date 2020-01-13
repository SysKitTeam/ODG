using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Office365;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface IUserGraphApiClient
    {
        Task<UserEntryCollection> GetAllTenantUsers(string tenantDomain);
        /// <summary>
        /// Creates users on tenant and returns successfully created users
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        Task<List<UserEntry>> CreateTenantUsers(IEnumerable<UserEntry> users);
    }
}

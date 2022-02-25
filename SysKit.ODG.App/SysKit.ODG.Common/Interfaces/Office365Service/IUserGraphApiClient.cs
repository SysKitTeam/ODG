using System.Collections.Generic;
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
        Task<O365CreationResult<UserEntry>> CreateTenantUsers(IEnumerable<UserEntry> users);

        /// <summary>
        /// Assigns a manager to a user. Returns true if there were errors
        /// </summary>
        /// <param name="managerSubordinatePairs"></param>
        /// <returns></returns>
        Task<bool> CreateUserManagers(List<ManagerSubordinatePair> managerSubordinatePairs);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Office365;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface IGroupGraphApiClient
    {
        /// <summary>
        /// Creates Unified groups and returns those that are successfully created
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        Task<List<UnifiedGroupEntry>> CreateUnifiedGroups(IEnumerable<UnifiedGroupEntry> groups, UserEntryCollection users);

        /// <summary>
        /// Creates MS Teams and returns those that are successfully created
        /// </summary>
        /// <param name="teams"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        Task<List<TeamEntry>> CreateTeams(IEnumerable<TeamEntry> teams, UserEntryCollection users);
    }
}

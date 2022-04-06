using System.Collections.Generic;
using System.Threading.Tasks;
using SysKit.ODG.Base.DTO;
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
        Task<CreatedGroupsResult> CreateUnifiedGroups(IEnumerable<UnifiedGroupEntry> groups, UserEntryCollection users);

        /// <summary>
        /// Creates MS Teams from existing groups and returns those that are successfully created
        /// </summary>
        /// <param name="teams"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        Task<O365CreationResult<TeamEntry>> CreateTeamsFromGroups(IEnumerable<TeamEntry> teams, UserEntryCollection users);

        /// <summary>
        /// Creates private channels for existing teams
        /// </summary>
        Task<bool> CreatePrivateTeamChannels(IEnumerable<PrivateTeamChannelCreationOptions> channels);

        /// <summary>
        /// Removes group owners. Key =>userId, Value => group from which to remove owner
        /// </summary>
        /// <param name="ownersMap"></param>
        /// <returns>Returns true if all owners where successfully removed</returns>
        Task<bool> RemoveGroupOwners(Dictionary<string, UnifiedGroupEntry> ownersMap);

        /// <summary>
        /// Tries to PERMANENTLY remove unified group. This will only remove the group, for site you need to call delete site
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>True if unified group was deleted</returns>
        Task<bool> DeleteUnifiedGroup(string groupId);
        /// <summary>
        /// Gets group ids from all teams on the tenant
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetAllTenantTeamIds();
        /// <summary>
        /// Gets ids of all members and owners from required teams
        /// </summary>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        Task<Dictionary<string, List<string>>> GetTeamMembers(List<string> groupIds);
    }
}

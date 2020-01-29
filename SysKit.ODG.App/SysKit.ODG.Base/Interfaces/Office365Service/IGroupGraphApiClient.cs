using System.Collections.Generic;
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
        Task<CreatedGroupsResult> CreateUnifiedGroups(IEnumerable<UnifiedGroupEntry> groups, UserEntryCollection users);

        /// <summary>
        /// Creates MS Teams from existing groups and returns those that are successfully created
        /// </summary>
        /// <param name="teams"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        Task<O365CreationResult<TeamEntry>> CreateTeamsFromGroups(IEnumerable<TeamEntry> teams, UserEntryCollection users);

        /// <summary>
        /// Creates private channels for existing groups
        /// </summary>
        /// <param name="teams"></param>
        /// <param name="users"></param>
        /// <returns>Returns true if all channels where created successfully</returns>
        Task<bool> CreatePrivateTeamChannels(IEnumerable<TeamEntry> teams, UserEntryCollection users);

        /// <summary>
        /// Removes group owners. Key =>userId, Value => group from which to remove owner
        /// </summary>
        /// <param name="ownersMap"></param>
        /// <returns>Returns true if all owners where successfully removed</returns>
        Task<bool> RemoveGroupOwners(Dictionary<string, UnifiedGroupEntry> ownersMap);
    }
}

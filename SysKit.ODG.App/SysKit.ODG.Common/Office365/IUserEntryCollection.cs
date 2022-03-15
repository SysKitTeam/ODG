using System.Collections.Generic;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Base.Office365
{
    public interface IUserEntryCollection
    {
        /// <summary>
        /// Returns UserEntry or null if member is not found
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        UserEntry FindMember(MemberEntry member);

        /// <summary>
        /// Returns {number} of random user member entries 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        IEnumerable<MemberEntry> GetRandomEntries(int number);

        /// <summary>
        /// Gets users and members for single group
        /// </summary>
        /// <returns></returns>
        (List<MemberEntry> members, List<MemberEntry> owners) GetMembersAndOwners();
    }
}
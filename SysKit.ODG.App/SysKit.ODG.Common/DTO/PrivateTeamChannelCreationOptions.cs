using System.Collections.Generic;

namespace SysKit.ODG.Base.DTO
{
    public class PrivateTeamChannelCreationOptions
    {
        public string GroupId { get; set; }
        public string ChannelName { get; set; }
        public List<string> MemberIds { get; set; }
        public List<string> OwnerIds { get; set; }

    }
}
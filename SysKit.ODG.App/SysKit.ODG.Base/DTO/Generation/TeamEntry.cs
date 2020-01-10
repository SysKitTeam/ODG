using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class TeamEntry: UnifiedGroupEntry
    {
        public List<TeamChannelEntry> Channels { get; set; }
        
        public TeamEntry()
        {
            IsTeam = true;
            Channels = new List<TeamChannelEntry>();
        }
    }
}

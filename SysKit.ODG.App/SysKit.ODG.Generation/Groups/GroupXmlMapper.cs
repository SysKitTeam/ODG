using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.XmlTemplate.Model;
using SysKit.ODG.Base.XmlTemplate.Model.Groups;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupXmlMapper
    {
        private readonly IMapper _mapper;

        public GroupXmlMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public TeamEntry MapToTeamEntry(string tenantDomainName, XmlTeam team)
        {
            var teamEntry = new TeamEntry();
            populateGroupEntry(team, teamEntry);

            foreach (var channel in team.Channels)
            {
                var channelEntry = new TeamChannelEntry(channel.DisplayName, channel.IsPrivate);
                if (channelEntry.IsPrivate)
                {
                    channelEntry.Members = channel.Members?.Select(m => new MemberEntry(m.Name)).ToList();
                    channelEntry.Owners = channel.Owners?.Select(m => new MemberEntry(m.Name)).ToList();
                }

                teamEntry.Channels.Add(channelEntry);
            }

            return teamEntry;
        }

        public UnifiedGroupEntry MapToUnifiedGroupEntry(string tenantDomainName, XmlUnifiedGroup unifiedGroup)
        {
            var groupEntry = new UnifiedGroupEntry();
            return populateGroupEntry(unifiedGroup, groupEntry);
        }

        private UnifiedGroupEntry populateGroupEntry(XmlUnifiedGroup unifiedGroup, UnifiedGroupEntry groupEntry)
        {
            groupEntry.DisplayName = unifiedGroup.DisplayName;
            groupEntry.MailNickname = unifiedGroup.Name;
            groupEntry.IsPrivate = unifiedGroup.IsPrivate;

            groupEntry.Members = unifiedGroup.Members?.Select(m => new MemberEntry(m.Name)).ToList();
            groupEntry.Owners = unifiedGroup.Owners?.Select(m => new MemberEntry(m.Name)).ToList();

            return groupEntry;
        }
    }
}

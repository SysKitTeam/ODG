﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.XmlCleanupTemplate;
using SysKit.ODG.Base.XmlTemplate.Model.Groups;
using SysKit.ODG.Generation.Sites;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupXmlMapper
    {
        private readonly IMapper _mapper;
        private readonly SharePointContentXmlMapper _sharePointContentXmlMapper;
        public GroupXmlMapper(IMapper mapper)
        {
            _mapper = mapper;
            _sharePointContentXmlMapper = new SharePointContentXmlMapper();
        }

        public TeamEntry MapToTeamEntry(XmlTeam team)
        {
            var teamEntry = new TeamEntry();
            populateGroupEntry(team, teamEntry);

            if (team.Channels.Any() != true)
            {
                return teamEntry;
            }

            foreach (var channel in team.Channels)
            {
                validateChannel(channel);
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

        public UnifiedGroupEntry MapToUnifiedGroupEntry(XmlUnifiedGroup unifiedGroup)
        {
            var groupEntry = new UnifiedGroupEntry();
            return populateGroupEntry(unifiedGroup, groupEntry);
        }

        public XmlDirectoryElement MapToDirectoryElement(GroupEntry groupEntry)
        {
            return _mapper.Map<GroupEntry, XmlDirectoryElement>(groupEntry);
        }

        private UnifiedGroupEntry populateGroupEntry(XmlUnifiedGroup unifiedGroup, UnifiedGroupEntry groupEntry)
        {
            validateGroup(unifiedGroup);
            groupEntry.DisplayName = unifiedGroup.DisplayName;
            groupEntry.MailNickname = unifiedGroup.Name;
            groupEntry.IsPrivate = unifiedGroup.IsPrivate;

            groupEntry.Members = unifiedGroup.Members?.Select(m => new MemberEntry(m.Name)).ToList();
            groupEntry.Owners = unifiedGroup.Owners?.Select(m => new MemberEntry(m.Name)).ToList();

            groupEntry.SPOwners = unifiedGroup.SPOwners?.Select(sa => new MemberEntry(sa.Name)).ToList() ?? new List<MemberEntry>();
            groupEntry.SPMembers = unifiedGroup.SPMembers?.Select(sa => new MemberEntry(sa.Name)).ToList() ?? new List<MemberEntry>();
            groupEntry.SPVisitors = unifiedGroup.SPVisitors?.Select(sa => new MemberEntry(sa.Name)).ToList() ?? new List<MemberEntry>();

            groupEntry.Content = _sharePointContentXmlMapper.MapToContentEntry(unifiedGroup.SharePointContent, true);

            return groupEntry;
        }

        private void validateGroup(XmlGroup xmlGroup)
        {
            if (string.IsNullOrEmpty(xmlGroup.DisplayName))
            {
                throw new XmlValidationException("Group DisplayName property is not defined");
            }

            if (string.IsNullOrEmpty(xmlGroup.Name))
            {
                throw new XmlValidationException("Group Name property is not defined");
            }
        }

        private void validateChannel(XmlTeamChannel xmlChannel)
        {
            if (string.IsNullOrEmpty(xmlChannel.DisplayName))
            {
                throw new XmlValidationException("Channel DisplayName property is not defined");
            }

            if (xmlChannel.IsPrivate && xmlChannel.Owners?.Any() == false)
            {
                throw new XmlValidationException("Private channel need at least one owner");
            }
        }
    }
}

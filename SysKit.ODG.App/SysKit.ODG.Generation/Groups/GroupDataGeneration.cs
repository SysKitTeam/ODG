using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using SysKit.ODG.Base;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.SampleData;
using SysKit.ODG.Base.Office365;
using SysKit.ODG.Base.Utils;
using SysKit.ODG.Base.XmlTemplate.Model;
using SysKit.ODG.Base.XmlTemplate.Model.Groups;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupDataGeneration : IGroupDataGeneration
    {
        private readonly IMapper _mapper;
        private readonly ISampleDataService _sampleDataService;
        private readonly GroupXmlMapper _groupXmlMapper;
        private readonly HashSet<string> _usedGroupUPNs = new HashSet<string>();

        public GroupDataGeneration(IMapper mapper, ISampleDataService sampleDataService)
        {
            _mapper = mapper;
            _sampleDataService = sampleDataService;
            _groupXmlMapper = new GroupXmlMapper(mapper);
        }

        public IEnumerable<UnifiedGroupEntry> CreateUnifiedGroupsAndTeams(GenerationOptions generationOptions, IUserEntryCollection userEntryCollection)
        {
            foreach (var group in createXmlUnifiedGroups(generationOptions))
            {
                yield return group;
            }

            foreach (var group in createRandomUnifiedGroups(generationOptions, userEntryCollection))
            {
                yield return group;
            }
        }

        private IEnumerable<UnifiedGroupEntry> createXmlUnifiedGroups(GenerationOptions generationOptions)
        {
            if (generationOptions.Template.Groups == null)
            {
                yield break;
            }

            foreach (var group in generationOptions.Template.Groups)
            {
                if (group is XmlTeam team)
                {
                    var teamEntry = _groupXmlMapper.MapToTeamEntry(team);
                    yield return teamEntry;
                    continue;
                }

                if (group is XmlUnifiedGroup unifiedGroup)
                {
                    var groupEntry = _groupXmlMapper.MapToUnifiedGroupEntry(unifiedGroup);
                    yield return groupEntry;
                }
            }
        }

        private IEnumerable<UnifiedGroupEntry> createRandomUnifiedGroups(GenerationOptions generationOptions, IUserEntryCollection userEntryCollection)
        {
            if (generationOptions.Template.RandomOptions?.NumberOfUnifiedGroups == null)
            {
                yield break;
            }

            for (int i = 0; i < generationOptions.Template.RandomOptions.NumberOfUnifiedGroups; i++)
            {
                yield return createSampleUnifiedGroupEntry(generationOptions, userEntryCollection);
            }

            for (int i = 0; i < generationOptions.Template.RandomOptions.NumberOfTeams; i++)
            {
                yield return createSampleTeamEntry(generationOptions, userEntryCollection);
            }
        }

        private TeamEntry createSampleTeamEntry(GenerationOptions generationOptions, IUserEntryCollection userEntryCollection)
        {
            var sampleTeam = new TeamEntry();
            populateSampleUnifiedGroupProperties(generationOptions, userEntryCollection, sampleTeam);
            var maxChannels = 10;
            var currentTeamChannels = RandomThreadSafeGenerator.Next(maxChannels);

            for (int i = 0; i < currentTeamChannels; i++)
            {
                var isPrivateChannel = RandomThreadSafeGenerator.Next(0, 100) > 80;
                var channelName = _sampleDataService.GetRandomValue(_sampleDataService.GroupNames);
                var channelEntry = new TeamChannelEntry(channelName, isPrivateChannel);

                if (channelEntry.IsPrivate)
                {
                    // for testing purposes I want both channel owners and members to be from group members
                    channelEntry.Owners = sampleTeam.Members.GetRandom(RandomThreadSafeGenerator.Next(3)).ToList();
                    channelEntry.Members = sampleTeam.Members.GetRandom(RandomThreadSafeGenerator.Next(5)).ToList();
                }

                sampleTeam.Channels.Add(channelEntry);
            }

            return sampleTeam;
        }

        private UnifiedGroupEntry createSampleUnifiedGroupEntry(GenerationOptions generationOptions, IUserEntryCollection userEntryCollection)
        {
            var sampleGroup = new UnifiedGroupEntry();
            populateSampleUnifiedGroupProperties(generationOptions, userEntryCollection, sampleGroup);
            return sampleGroup;
        }

        private void populateSampleUnifiedGroupProperties(GenerationOptions generationOptions,
            IUserEntryCollection userEntryCollection, UnifiedGroupEntry sampleGroup)
        {
            populateSampleGroupProperties(sampleGroup, userEntryCollection, generationOptions.Template.RandomOptions);
            sampleGroup.IsPrivate = RandomThreadSafeGenerator.Next(0, 100) > 70;

            string originalGroupMailNick = Regex.Replace(sampleGroup.DisplayName, @"[^a-z0-9]", "");
            // sample values have entries that can produce null here
            string groupMailNick = string.IsNullOrEmpty(originalGroupMailNick) ? "testgroup" : originalGroupMailNick;

            int i = 0;
            while (_usedGroupUPNs.Contains(groupMailNick))
            {
                groupMailNick = $"{originalGroupMailNick}{++i}";
            }

            sampleGroup.MailNickname = groupMailNick;
            _usedGroupUPNs.Add(groupMailNick);
        }

        private void populateSampleGroupProperties(GroupEntry groupEntry, IUserEntryCollection userEntryCollection, XmlRandomOptions generationOptions)
        {
            groupEntry.DisplayName = _sampleDataService.GetRandomValue(_sampleDataService.GroupNames);
            groupEntry.Owners = userEntryCollection.GetRandomEntries(RandomThreadSafeGenerator.Next(generationOptions.MaxNumberOfOwnersPerGroup))
                .ToList();
            groupEntry.Members = userEntryCollection.GetRandomEntries(RandomThreadSafeGenerator.Next(generationOptions.MaxNumberOfMembersPerGroup))
                .ToList();
        }
    }
}

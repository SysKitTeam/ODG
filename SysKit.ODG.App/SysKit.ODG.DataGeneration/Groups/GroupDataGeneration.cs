using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using SysKit.ODG.Base;
using SysKit.ODG.Base.DTO;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.SampleData;
using SysKit.ODG.Base.Office365;
using SysKit.ODG.Base.Utils;
using SysKit.ODG.Base.XmlCleanupTemplate;
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

        public IEnumerable<XmlDirectoryElement> CreateDirectoryElements(IEnumerable<GroupEntry> groups)
        {
            if (groups == null)
            {
                yield break;
            }

            foreach (var group in groups)
            {
                yield return _groupXmlMapper.MapToDirectoryElement(group);
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


            return sampleTeam;
        }

        public IEnumerable<PrivateTeamChannelCreationOptions> CreatePrivateChannels(Dictionary<string, List<string>> teamMembershipLookup)
        {
            var teamIds = teamMembershipLookup.Keys;


            return teamIds.Select(teamId =>
            {
                var ownerIds = teamMembershipLookup[teamId].GetRandom(RandomThreadSafeGenerator.Next(1, 3)).ToList();
                var memberIds = teamMembershipLookup[teamId].GetRandom(RandomThreadSafeGenerator.Next(1, 8)).Where(m => !ownerIds.Contains(m)).ToList();

                return new PrivateTeamChannelCreationOptions
                {
                    ChannelName = getChannelName(),
                    GroupId = teamId,
                    OwnerIds = ownerIds,
                    MemberIds = memberIds
                };
            });

        }

        private string getChannelName()
        {
            // 50 is max
            var maxChannelName = 49;
            var cleanChannelName = _sampleDataService.GetRandomValue(_sampleDataService.GroupNamesPart1,
                _sampleDataService.GroupNamesPart2, false);

            return
                cleanChannelName.Length <= maxChannelName
                ? cleanChannelName
                : cleanChannelName.Substring(0, maxChannelName);
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
            sampleGroup.IsPrivate = RandomThreadSafeGenerator.Next(0, 100) > 5;

            string originalGroupMailNick = Regex.Replace(sampleGroup.DisplayName.ToLower(), @"[^a-z0-9]", "");
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

            var memberAndOwnerGenerationResult = userEntryCollection.GetMembersAndOwners(generationOptions.CreateDepartmentTeams);
            groupEntry.DisplayName = memberAndOwnerGenerationResult.IsDepartmentTeam
                ? memberAndOwnerGenerationResult.DepartmentTeamName
                : _sampleDataService.GetRandomValue(_sampleDataService.GroupNamesPart1, _sampleDataService.GroupNamesPart2, false);
            groupEntry.Owners = memberAndOwnerGenerationResult.Owners;
            groupEntry.Members = memberAndOwnerGenerationResult.Members;
            groupEntry.Template = memberAndOwnerGenerationResult.Template;
        }
    }
}

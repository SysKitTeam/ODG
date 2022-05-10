using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using SysKit.ODG.Base;
using SysKit.ODG.Base.DTO;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Enums;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Interfaces.SampleData;
using SysKit.ODG.Base.Office365;
using SysKit.ODG.Base.Utils;
using SysKit.ODG.Base.XmlCleanupTemplate;
using SysKit.ODG.Base.XmlTemplate.Model;
using SysKit.ODG.Base.XmlTemplate.Model.Groups;
using SysKit.ODG.Common.DTO.Generation;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupDataGeneration : IGroupDataGeneration
    {
        private readonly IMapper _mapper;
        private readonly ISampleDataService _sampleDataService;
        private readonly GroupXmlMapper _groupXmlMapper;
        private readonly HashSet<string> _usedGroupUPNs = new HashSet<string>();
        private readonly ISharePointFileService _sharePointFileService;

        public GroupDataGeneration(IMapper mapper, ISampleDataService sampleDataService, ISharePointFileService sharePointFileService)
        {
            _mapper = mapper;
            _sampleDataService = sampleDataService;
            _sharePointFileService = sharePointFileService;
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

        private const int MinNumberOfRootFolders = 4;
        private const int MaxNumberOfRootFolders = 6;
        private const int MaxNumberOfRootFiles = 4;

        private const int MinNumberOfFoldersPerLevel = 4;
        private const int MaxNumberOfFoldersPerLevel = 6;
        private const int MaxNumberOfFilesPerLevel = 4; // This isn't respected for the deepest level of hierarchy

        public List<ContentEntry> GenerateDocumentsFolderStructure(int itemsPerSite, IUserEntryCollection userEntryCollection, List<string> groupEmails)
        {
            var itemCounter = 0;
            var folderCounter = 0;
            var rootNodes = new List<ContentEntry>();

            var numberOfRootFolders = RandomThreadSafeGenerator.Next(MinNumberOfRootFolders, MaxNumberOfRootFolders);
            var numberOfRootFiles = RandomThreadSafeGenerator.Next(MaxNumberOfRootFiles);
            rootNodes.AddRange(generateNodeList(numberOfRootFolders, numberOfRootFiles, userEntryCollection, groupEmails));
            itemCounter += rootNodes.Count;


            var folderQueue = new Queue<ContentEntry>();
            foreach (var rootFolder in rootNodes.Where(n => n.Type == ContentTypeEnum.Folder))
            {
                folderQueue.Enqueue(rootFolder);
                folderCounter++;
            }

            while (itemCounter <= itemsPerSite)
            {
                var numberOfFolders = folderCounter * 10 < itemsPerSite ? RandomThreadSafeGenerator.Next(MinNumberOfFoldersPerLevel, MaxNumberOfFoldersPerLevel) : 0;
                var numberOfFiles = folderCounter * 10 < itemsPerSite ? RandomThreadSafeGenerator.Next(MaxNumberOfFilesPerLevel) : ((itemsPerSite - itemCounter) / folderQueue.Count) + 1;

                var currentFolder = folderQueue.Dequeue();
                currentFolder.Children = generateNodeList(numberOfFolders, numberOfFiles, userEntryCollection, groupEmails);

                foreach (var childFolder in currentFolder.Children.Where(c => c.Type == ContentTypeEnum.Folder))
                {
                    folderQueue.Enqueue(childFolder);
                    folderCounter++;
                }

                itemCounter += currentFolder.Children.Count;

            }

            return rootNodes;
        }

        private List<ContentEntry> generateNodeList(int numberOfFolders, int numberOfFiles, IUserEntryCollection userEntryCollection, List<string> groupEmails)
        {
            var rootNodes = new List<ContentEntry>();

            for (var i = 0; i < numberOfFolders; i++)
            {
                var name = _sampleDataService.GetRandomValue(_sampleDataService.GroupNamesPart1,
                    _sampleDataService.GroupNamesPart1, _sampleDataService.GroupNamesPart2);
                var sharingLinks = getLinksForListItem(userEntryCollection);
                var permissions = getRoleAssignmentsForListItem(userEntryCollection, groupEmails);
                var folder = new ContentEntry(name, ContentTypeEnum.Folder)
                {
                    HasUniqueRoleAssignments = sharingLinks.Count > 0 || permissions.Keys.Count > 0,
                    Assignments = permissions,
                    Children = new List<ContentEntry>(),
                    SharingLinks = sharingLinks,
                    CopyFromParent = true
                };
                rootNodes.Add(folder);
            }

            for (var i = 0; i < numberOfFiles; i++)
            {
                var name = _sampleDataService.GetRandomValue(_sampleDataService.GroupNamesPart1,
                    _sampleDataService.GroupNamesPart1, _sampleDataService.GroupNamesPart2);
                var sharingLinks = getLinksForListItem(userEntryCollection);
                var permissions = getRoleAssignmentsForListItem(userEntryCollection, groupEmails);
                var file = new FileEntry(name, getFileExtension())
                {
                    HasUniqueRoleAssignments = sharingLinks.Count > 0 || permissions.Keys.Count > 0,
                    Assignments = permissions,
                    SharingLinks = sharingLinks,
                    CopyFromParent = true
                };
                rootNodes.Add(file);
            }

            return rootNodes;
        }

        private const int DirectRoleAssignmentPercentage = 2;
        private const int DirectRoleAssignmentUserPercentage = 95;
        private Dictionary<RoleTypeEnum, HashSet<MemberEntry>> getRoleAssignmentsForListItem(IUserEntryCollection userEntryCollection, List<string> groupEmails)
        {
            var roleAssignments = new Dictionary<RoleTypeEnum, HashSet<MemberEntry>>();
            var hasDirectAssignment = RandomThreadSafeGenerator.Next(100) < DirectRoleAssignmentPercentage;
            if (!hasDirectAssignment)
            {
                return roleAssignments;
            }

            var isUserAssigned = RandomThreadSafeGenerator.Next(100) < DirectRoleAssignmentUserPercentage;
            if (isUserAssigned)
            {
                var user = getEnabledMemberEntry(userEntryCollection);
                if (user == null)
                {
                    return roleAssignments;
                }
                var roleType = (RoleTypeEnum)RandomThreadSafeGenerator.Next(1, 4);
                roleAssignments.Add(roleType, new HashSet<MemberEntry>() { user });

            }
            else
            {
                var groupEmail = groupEmails.GetRandom(1).FirstOrDefault();
                var roleType = (RoleTypeEnum)RandomThreadSafeGenerator.Next(1, 4);
                roleAssignments.Add(roleType, new HashSet<MemberEntry>() { new MemberEntry(groupEmail) });
            }

            return roleAssignments;
        }

        private MemberEntry getEnabledMemberEntry(IUserEntryCollection userEntryCollection)
        {
            for (var i = 0; i < 50; i++)
            {
                var member = userEntryCollection.GetRandomEntries(1).FirstOrDefault();
                var user = userEntryCollection.FindMember(member);
                if (user.AccountEnabled.HasValue && user.AccountEnabled.Value)
                {
                    return member;
                }
            }

            return null;
        }

        private const int SharingLinkPercentage = 2;
        private List<SharingLinkEntry> getLinksForListItem(IUserEntryCollection userEntryCollection)
        {
            var hasALink = RandomThreadSafeGenerator.Next(100) < SharingLinkPercentage;
            var hasASecondLink = hasALink && RandomThreadSafeGenerator.Next(100) < SharingLinkPercentage;

            var sharingLinks = new List<SharingLinkEntry>();
            if (hasALink)
            {
                sharingLinks.Add(getSharingLink(userEntryCollection));
            }

            if (hasASecondLink)
            {
                sharingLinks.Add(getSharingLink(userEntryCollection));
            }

            return sharingLinks;
        }

        private const int IsEditLinkPercentage = 50;
        private const int IsAnonymousLinkPercentage = 2;
        private const int IsSpecificLinkPercentage = 35;
        private SharingLinkEntry getSharingLink(IUserEntryCollection userEntryCollection)
        {
            var rand = RandomThreadSafeGenerator.Next(100);
            var isEdit = RandomThreadSafeGenerator.Next(100) < IsEditLinkPercentage;
            if (rand < IsAnonymousLinkPercentage)
            {
                return new SharingLinkEntry()
                {
                    SharingLinkType = SharingLinkType.Anonymous,
                    IsEdit = isEdit
                };
            }

            if (rand < IsSpecificLinkPercentage)
            {
                var user = userEntryCollection.GetRandomEntries(1).FirstOrDefault();

                return new SpecificSharingLinkEntry()
                {
                    SharingLinkType = SharingLinkType.Specific,
                    IsEdit = isEdit,
                    SharedWithEmail = user?.Name
                };
            }

            return new SharingLinkEntry()
            {
                SharingLinkType = SharingLinkType.Company,
                IsEdit = isEdit
            };
        }

        private string getFileExtension()
        {
            return _sharePointFileService.GetFileExtensions().GetRandom(1).First();
        }

    }
}

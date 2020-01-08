using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.SampleData;
using SysKit.ODG.Base.Utils;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupDataGeneration : IGroupDataGeneration
    {
        private readonly IMapper _mapper;
        private readonly ISampleDataService _sampleDataService;
        private readonly GroupXmlMapper _groupXmlMapper;

        public GroupDataGeneration(IMapper mapper, ISampleDataService sampleDataService)
        {
            _mapper = mapper;
            _sampleDataService = sampleDataService;
            _groupXmlMapper = new GroupXmlMapper(mapper);
        }

        public IEnumerable<GroupEntry> CreateGroups(GenerationOptions generationOptions)
        {
            foreach (var group in createRandomGroups(generationOptions))
            {
                yield return group;
            }
        }

        private IEnumerable<GroupEntry> createRandomGroups(GenerationOptions generationOptions)
        {
            if (generationOptions.Template.RandomOptions?.NumberOfUnifiedGroups == null)
            {
                yield break;
            }

            for (int i = 0; i < generationOptions.Template.RandomOptions.NumberOfUnifiedGroups; i++)
            {
                yield return createSampleUnifiedGroupEntry(generationOptions);
            }
        }

        private UnifiedGroupEntry createSampleUnifiedGroupEntry(GenerationOptions generationOptions)
        {
            var sampleGroup = new UnifiedGroupEntry();

            populateSampleGroupProperties(sampleGroup);
            sampleGroup.IsPrivate = RandomThreadSafeGenerator.Next(0, 100) > 70;

            return sampleGroup;
        }

        private void populateSampleGroupProperties(GroupEntry groupEntry)
        {
            groupEntry.DisplayName = _sampleDataService.GetRandomValue(_sampleDataService.GroupNames);
        }
    }
}

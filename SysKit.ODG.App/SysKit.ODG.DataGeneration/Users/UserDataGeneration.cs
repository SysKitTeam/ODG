using System;
using System.Collections.Generic;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.SampleData;
using SysKit.ODG.Base.XmlCleanupTemplate;
using SysKit.ODG.Common.Interfaces.SampleData;

namespace SysKit.ODG.Generation.Users
{
    public class UserDataGeneration : IUserDataGeneration
    {
        private readonly IMapper _mapper;
        private readonly ISampleDataService _sampleDataService;
        private readonly UserXmlMapper _userXmlMapper;
        private readonly IJobHierarchyService _jobHierarchyService;

        private readonly HashSet<string> _sampleUserUPNs = new HashSet<string>();

        public UserDataGeneration(IMapper mapper, ISampleDataService sampleDataService, IJobHierarchyService jobHierarchyService)
        {
            _mapper = mapper;
            _sampleDataService = sampleDataService;
            _jobHierarchyService = jobHierarchyService;
            _userXmlMapper = new UserXmlMapper(mapper);
        }

        public IEnumerable<UserEntry> CreateUsers(UserGenerationOptions generationOptions)
        {
            foreach (var xmlUser in createXmlUsers(generationOptions))
            {
                yield return xmlUser;
            }

            foreach (var xmlUser in createRandomUsers(generationOptions))
            {
                yield return xmlUser;
            }
        }

        public IEnumerable<XmlDirectoryElement> CreateDirectoryElements(IEnumerable<UserEntry> users)
        {
            if (users == null)
            {
                yield break;
            }

            foreach (var user in users)
            {
                yield return _userXmlMapper.MapToDirectoryElement(user);
            }
        }

        private IEnumerable<UserEntry> createXmlUsers(UserGenerationOptions generationOptions)
        {
            if (generationOptions.Users == null)
            {
                yield break;
            }

            foreach (var xmlUser in generationOptions.Users)
            {
                var userEntry = _userXmlMapper.MapToUserEntry(generationOptions.TenantDomain, xmlUser);
                var defaultValues = createSampleUserEntry(generationOptions);
                yield return _mapper.Map(userEntry, defaultValues);
            }
        }

        private IEnumerable<UserEntry> createRandomUsers(UserGenerationOptions generationOptions)
        {
            if (generationOptions.RandomOptions?.NumberOfUsers == null)
            {
                yield break;
            }

            for (int i = 0; i < generationOptions.RandomOptions.NumberOfUsers; i++)
            {
                yield return createSampleUserEntry(generationOptions);
            }
        }

        /// <summary>
        /// Returns user entry populated with sample data
        /// </summary>
        /// <returns></returns>
        private UserEntry createSampleUserEntry(UserGenerationOptions generationOptions)
        {
            string fakeDisplayName;
            RandomValueWithComponents fakeName;

            // just so we dont deadlock
            int count = 0;

            do
            {
                fakeName = _sampleDataService.GetRandomValueWithComponents(_sampleDataService.FirstNames,
                    _sampleDataService.LastNames);
                fakeDisplayName = fakeName.RandomValue;
                count++;
            } while (_sampleUserUPNs.Contains(fakeDisplayName) || count > 100);

            if (fakeDisplayName == null)
            {
                throw new ArgumentNullException("Unable to generate sample display name.");
            }

            var company = _sampleDataService.GetRandomValue(_sampleDataService.CompanyNames);
            var department = _sampleDataService.GetRandomValue(_sampleDataService.DepartmentNames);
            var (hierarchyLevel, jobTitle) = _jobHierarchyService.GetHierarchyLevelAndJobTitle(company, department);
            var address = _sampleDataService.GetRandomAddress(_sampleDataService.StreetAddresses);

            _sampleUserUPNs.Add(fakeDisplayName);
            return new UserEntry
            {
                DisplayName = fakeDisplayName,
                GivenName = fakeName.Components[0],
                Surname = fakeName.Components[1],
                MailNickname = createMailNickName(fakeDisplayName),
                Password = generationOptions.DefaultPassword,
                UserPrincipalName = $"{createMailNickName(fakeDisplayName)}@{generationOptions.TenantDomain}",
                AccountEnabled = DateTime.Now.Ticks % 7 != 0,
                Department = department,
                CompanyName = company,
                OfficeLocation = $"{address.City} Office",
                JobTitle = jobTitle,
                HierarchyLevel = hierarchyLevel,
                StreetAddress = address.StreetAddress,
                City = address.City,
                PostalCode = address.PostalCode,
                State = address.State,
                Country = address.Country
            };
        }

        private string createMailNickName(string displayName)
        {
            var nameParts = displayName.Split(' ');
            var mailNicknameParts = new List<string>();

            foreach (var namePart in nameParts)
            {
                mailNicknameParts.Add(Char.ToLower(namePart[0]) + namePart.Substring(1));
            }

            return string.Join(".", mailNicknameParts);
        }
    }
}

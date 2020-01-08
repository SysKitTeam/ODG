using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.SampleData;
using SysKit.ODG.Base.XmlTemplate.Model;

namespace SysKit.ODG.Generation.Users
{
    public class UserDataGeneration : IUserDataGeneration
    {
        private readonly IMapper _mapper;
        private readonly ISampleDataService _sampleDataService;
        private readonly UserXmlMapper _userXmlMapper;

        private readonly HashSet<string> _sampleUserUPNs = new HashSet<string>();

        public UserDataGeneration(IMapper mapper, ISampleDataService sampleDataService)
        {
            _mapper = mapper;
            _sampleDataService = sampleDataService;
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

        private IEnumerable<UserEntry> createXmlUsers(UserGenerationOptions generationOptions)
        {
            var xmlSpecification = generationOptions.UserOptions;
            if (xmlSpecification?.Users == null)
            {
                yield break;
            }

            foreach (var xmlUser in xmlSpecification.Users)
            {
                var userEntry = _userXmlMapper.MapToUserEntry(xmlUser);
                var defaultValues = createSampleUserEntry(generationOptions);
                yield return _mapper.Map(userEntry, defaultValues);
            }
        }

        private IEnumerable<UserEntry> createRandomUsers(UserGenerationOptions generationOptions)
        {
            if (generationOptions.UserOptions?.RandomOptions?.NumberOfUsers == null)
            {
                yield break;
            }

            for (int i = 0; i < generationOptions.UserOptions.RandomOptions.NumberOfUsers; i++)
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
            // just so we dont deadlock
            int count = 0;

            do
            {
                fakeDisplayName =
                    _sampleDataService.GetRandomValue(_sampleDataService.FirstNames, _sampleDataService.LastNames);
                count++;
            } while (_sampleUserUPNs.Contains(fakeDisplayName) || count > 100);

            if (fakeDisplayName == null)
            {
                throw new ArgumentNullException("Unable to generate sample display name.");
            }

            _sampleUserUPNs.Add(fakeDisplayName);
            return new UserEntry
            {
                DisplayName = fakeDisplayName,
                MailNickname = createMailNickName(fakeDisplayName),
                Password = generationOptions.DefaultPassword,
                UserPrincipalName = $"{createMailNickName(fakeDisplayName)}@{generationOptions.TenantDomain}",
                AccountEnabled = DateTime.Now.Ticks % 7 != 0
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

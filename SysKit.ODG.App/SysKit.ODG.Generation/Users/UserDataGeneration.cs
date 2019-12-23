using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.XMLSpecification.Model;

namespace SysKit.ODG.Generation.Users
{
    public class UserDataGeneration
    {
        private readonly IMapper _mapper;

        public UserDataGeneration(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IEnumerable<UserEntry> CreateUsers(XmlODGSpecification xmlSpecification)
        {
            if (xmlSpecification?.UserCollection?.Users == null)
            {
                yield break;
            }

            foreach (var xmlUser in xmlSpecification.UserCollection.Users)
            {
                yield return mapXmlToUserEntry(xmlUser);
            }
        }

        private UserEntry mapXmlToUserEntry(XmlUser xmlUser)
        {
            var userEntry = _mapper.Map<XmlUser, UserEntry>(xmlUser);
            var defaultValues = createSampleUserEntry();
            return _mapper.Map(userEntry, defaultValues);
        }

        /// <summary>
        /// Returns user entry populated with sample data
        /// </summary>
        /// <returns></returns>
        private UserEntry createSampleUserEntry()
        {
            return new UserEntry
            {
                DisplayName = "funny guy",
                MailNickname = "funny guy 2"
            };
        }
    }
}

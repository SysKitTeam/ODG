using System;
using System.Collections.Generic;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.XmlTemplate.Model;

namespace SysKit.ODG.Generation.Users
{
    public class UserXmlMapper
    {
        private readonly IMapper _mapper;

        public UserXmlMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public UserEntry MapToUserEntry(XmlUser xmlUser)
        {
            var userEntry = new UserEntry();

            userEntry.AccountEnabled = xmlUser.AccountEnabled;
            userEntry.UserPrincipalName = xmlUser.Name;

            var mailNickname = xmlUser.Name.Split('@')[0];

            userEntry.MailNickname = mailNickname;
            userEntry.DisplayName = createDisplayName(mailNickname);

            return userEntry;
        }

        /// <summary>
        /// Splits mail nickname on '.' and creates a display name => adele.vance => Adele Vance
        /// </summary>
        /// <param name="mailNickname"></param>
        /// <returns></returns>
        private string createDisplayName(string mailNickname)
        {
            var displayNamePats = new List<string>();
            var nameParts = mailNickname.Split('.');

            foreach (var namePart in nameParts)
            {
                displayNamePats.Add(Char.ToUpper(namePart[0]) + namePart.Substring(1));
            }

            return string.Join(" ", displayNamePats.ToArray());
        } 

        private void validateXmlUser(XmlUser xmlUser)
        {
            // TODO: validation
            // name is defined and has @
        }
    }
}

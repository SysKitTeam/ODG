using System;
using System.Collections.Generic;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.XmlTemplate.Model;

namespace SysKit.ODG.Generation.Users
{
    public class UserXmlMapper
    {
        public UserXmlMapper()
        {

        }

        public UserEntry MapToUserEntry(string tenantDomainName, XmlUser xmlUser)
        {
            var userEntry = new UserEntry();

            validateXmlUser(xmlUser);
            userEntry.AccountEnabled = !xmlUser.AccountDisabled;
            userEntry.UserPrincipalName = xmlUser.Name.Contains("@") ? xmlUser.Name : $"{xmlUser.Name}@{tenantDomainName}";

            var mailNickname = userEntry.UserPrincipalName.Split('@')[0];

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
            if (string.IsNullOrEmpty(xmlUser.Name))
            {
                throw new XmlValidationException("User Name property is not defined");
            }
        }
    }
}

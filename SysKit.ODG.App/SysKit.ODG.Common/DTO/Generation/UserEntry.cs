﻿namespace SysKit.ODG.Base.DTO.Generation
{
    public class UserEntry
    {
        /// <summary>
        /// User Id from Azure AD
        /// </summary>
        public string Id { get; set; }
        public bool? AccountEnabled { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string MailNickname { get; set; }
        public string UserPrincipalName { get; set; }
        public string Password { get; set; }
        public string Department { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string OfficeLocation { get; set; }

        public bool? SetUserPhoto { get; set; }

    }
}

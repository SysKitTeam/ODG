using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class UserEntry
    {
        public bool AccountEnabled { get; set; }
        public string DisplayName { get; set; }
        public string MailNickname { get; set; }
        public string UserPrincipalName { get; set; }
        /// <summary>
        /// If not set default password will be used
        /// </summary>
        public string Password { get; set; }

    }
}

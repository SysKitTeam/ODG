using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Interfaces.Model
{
    public interface IUser
    {
        bool? AccountEnabled { get; set; }
        string DisplayName { get; set; }
        string MailNickname { get; set; }
        string UserPrincipalName { get; set; }
        /// <summary>
        /// If not set default password will be used
        /// </summary>
        string Password { get; set; }
        bool? SetUserPhoto { get; set; }
    }
}

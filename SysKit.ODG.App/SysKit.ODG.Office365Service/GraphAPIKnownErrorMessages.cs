using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysKit.ODG.Office365Service
{
    /// <summary>
    /// Collection of known Graph API errors
    /// </summary>
    public static class GraphAPIKnownErrorMessages
    {
        public static string UserAlreadyExists =>
            "Another object with the same value for property userPrincipalName already exists.";

        public static string GroupAlreadyExists =>
            "Another object with the same value for property mailNickname already exists.";

        public static string GroupProvisionError => "Resource provisioning is in progress";

        public static string GroupProvisionError1 => "Unable to provision resource";

        public static string TeamProvisionError => "No team found with Group Id";

        public static string GuestUserGroupOwnerError =>
            "As per tenant wide policy, guest users are not allowed to be owner of an unified group.";
    }
}

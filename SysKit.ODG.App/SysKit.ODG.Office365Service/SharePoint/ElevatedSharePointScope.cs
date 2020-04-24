using System;
using System.Collections.Generic;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core.Entities;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Office365Service.SharePoint
{
    /// <summary>
    /// This will temporarily give user admin rights on site so he can execute other actions
    /// </summary>
    public class ElevatedSharePointScope : IDisposable
    {
        private readonly SiteEntry _site;
        private readonly SimpleUserCredentials _userCredentials;
        private readonly bool _wasUserAdmin;

        public ElevatedSharePointScope(SiteEntry site, SimpleUserCredentials userCredentials)
        {
            _site = site;
            _userCredentials = userCredentials;
            _wasUserAdmin = isUserAdmin();

            // set user as admin (won't check if user was admin, don't really care)
            using (var rootContext = SharePointUtils.CreateAdminContext(_userCredentials))
            {
                Tenant tenant = new Tenant(rootContext);
                tenant.AddAdministrators(new List<UserEntity> { new UserEntity { LoginName = _userCredentials.Username } }, new Uri(site.Url), false);
            }
        }

        private bool isUserAdmin()
        {
            var userAdmins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            userAdmins.Add(SharePointUtils.GetLoginNameFromEntry(_site.Owner, _site.Url));
            _site.SiteAdmins?.ForEach(admin => userAdmins.Add(SharePointUtils.GetLoginNameFromEntry(admin, _site.Url)));

            return userAdmins.Contains(_userCredentials.Username);
        }

        public void Dispose()
        {
            // remove user as admin
            if (!_wasUserAdmin)
            {
                using (var rootContext = SharePointUtils.CreateAdminContext(_userCredentials))
                {
                    Tenant tenant = new Tenant(rootContext);
                    tenant.SetSiteAdmin(_site.Url, _userCredentials.Username, false);
                    tenant.Context.ExecuteQueryRetry();
                }
            }
        }
    }
}

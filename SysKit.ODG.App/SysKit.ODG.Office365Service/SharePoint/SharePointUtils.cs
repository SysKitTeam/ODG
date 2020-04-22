using System;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Office365Service.SharePoint
{
    public static class SharePointUtils
    {
        /// <summary>
        /// Creates Client Context
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="userCredentials"></param>
        /// <returns></returns>
        public static ClientContext CreateClientContext(string siteUrl, SimpleUserCredentials userCredentials)
        {
            var manager = new AuthenticationManager();
            return manager.GetSharePointOnlineAuthenticatedContextTenant(siteUrl, userCredentials.Username, userCredentials.Password);
        }

        /// <summary>
        /// Creates admin portal context
        /// </summary>
        /// <param name="userCredentials"></param>
        /// <returns></returns>
        public static ClientContext CreateAdminContext(SimpleUserCredentials userCredentials)
        {
            var manager = new AuthenticationManager();
            return manager.GetSharePointOnlineAuthenticatedContextTenant(getAdminUrl(userCredentials), userCredentials.Username, userCredentials.Password);
        }

        /// <summary>
        /// Returns LoginName from MemberEntry
        /// </summary>
        /// <param name="memberEntry"></param>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        public static string GetLoginNameFromEntry(MemberEntry memberEntry, string siteUrl)
        {
            if (memberEntry.IsFQDN)
            {
                return memberEntry.Name;
            }

            var host = new Uri(siteUrl);
            return $"{memberEntry.Name}@{host.Host.Replace(".sharepoint.com", ".onmicrosoft.com")}";
        }

        private static string getTenantUrl(string siteUrl)
        {
            var host = new Uri(siteUrl);
            return $"{host.Scheme}://{host.Host}";
        }

        private static string getAdminUrl(string siteUrl)
        {
            var host = new Uri(siteUrl);
            return $"{host.Scheme}://{host.Host.Replace(".sharepoint.com", "-admin.sharepoint.com")}";
        }

        private static string getAdminUrl(SimpleUserCredentials userCredentials)
        {
            var domain = userCredentials.Username.Split('@')[1];
            return $"https://{domain.Replace(".onmicrosoft.com", "-admin.sharepoint.com")}";
        }
    }
}

using System;

namespace SysKit.ODG.Base.Exceptions
{
    public class SiteAlreadyExists : Exception
    {
        public SiteAlreadyExists(string siteUrl) : base($"Site: {siteUrl} already exists on SharePoint")
        {

        }
    }
}

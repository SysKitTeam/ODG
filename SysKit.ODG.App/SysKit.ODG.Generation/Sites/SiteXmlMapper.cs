using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.XmlTemplate.Model.Sites;

namespace SysKit.ODG.Generation.Sites
{
    public class SiteXmlMapper
    {
        public SiteEntry MapToSiteEntry(string tenant, XmlSite site)
        {
            validateSite(site);
            var siteEntry = new SiteEntry
            {
                Title = site.Title,
                Url = $"https://{tenant.Replace(".onmicrosoft.com", ".sharepoint.com")}/sites/{site.RelativeUrl.TrimStart('/')}"
            };

            siteEntry.Owner = new MemberEntry(site.PrimaryAdmin.Name);
            siteEntry.SiteAdmins = site.SiteAdmins?.Select(sa => new MemberEntry(sa.Name)).ToList() ?? new List<MemberEntry>();

            return siteEntry;
        }

        private void validateSite(XmlSite site)
        {
            if (string.IsNullOrEmpty(site.Title))
            {
                throw new XmlValidationException($"Site {nameof(site.Title)} property is not defined");
            }

            if (string.IsNullOrEmpty(site.RelativeUrl))
            {
                throw new XmlValidationException($"Site {nameof(site.RelativeUrl)} property is not defined");
            }

            if (site.PrimaryAdmin is null)
            {
                throw new XmlValidationException($"Site {nameof(site.PrimaryAdmin)} property is not defined");
            }
        }
    }
}

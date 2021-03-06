﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Enums;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.XmlCleanupTemplate;
using SysKit.ODG.Base.XmlTemplate.Model.Sites;

namespace SysKit.ODG.Generation.Sites
{
    public class SiteXmlMapper
    {
        private readonly SharePointContentXmlMapper _sharePointContentXmlMapper;
        public SiteXmlMapper()
        {
            _sharePointContentXmlMapper = new SharePointContentXmlMapper();
        }

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

            siteEntry.SPOwners = site.SPOwners?.Select(sa => new MemberEntry(sa.Name)).ToList() ?? new List<MemberEntry>();
            siteEntry.SPMembers = site.SPMembers?.Select(sa => new MemberEntry(sa.Name)).ToList() ?? new List<MemberEntry>();
            siteEntry.SPVisitors = site.SPVisitors?.Select(sa => new MemberEntry(sa.Name)).ToList() ?? new List<MemberEntry>();

            siteEntry.Content = _sharePointContentXmlMapper.MapToContentEntry(site.SharePointContent, true);

            return siteEntry;
        }

        public XmlDirectoryElement MapToDirectoryElement(SiteEntry siteEntry)
        {
            return new XmlDirectoryElement
            {
                Type = DirectoryElementTypeEnum.Site,
                Url = siteEntry.Url,
                DisplayName = siteEntry.Title
            };
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

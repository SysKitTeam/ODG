﻿using System.Collections.Generic;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Interfaces.Generation;

namespace SysKit.ODG.Generation.Sites
{
    public class SiteDataGeneration : ISiteDataGeneration
    {
        private readonly SiteXmlMapper _siteXmlMapper;
        public SiteDataGeneration()
        {
            _siteXmlMapper = new SiteXmlMapper();
        }

        public IEnumerable<SiteEntry> CreateSites(GenerationOptions generationOptions)
        {
            foreach (var siteEntry in createXmlSites(generationOptions))
            {
                yield return siteEntry;
            }
        }

        private IEnumerable<SiteEntry> createXmlSites(GenerationOptions generationOptions)
        {
            if (generationOptions.Template.Sites == null)
            {
                yield break;
            }

            foreach (var site in generationOptions.Template.Sites)
            {
                yield return _siteXmlMapper.MapToSiteEntry(generationOptions.TenantDomain, site);
            }
        }
    }
}

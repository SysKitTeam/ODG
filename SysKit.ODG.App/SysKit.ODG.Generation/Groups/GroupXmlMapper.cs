using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.XmlTemplate.Model;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupXmlMapper
    {
        private readonly IMapper _mapper;

        public GroupXmlMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public UnifiedGroupEntry MapToUnifiedGroupEntry(string tenantDomainName, XmlUnifiedGroup unifiedGroup)
        {
            var groupEntry = new UnifiedGroupEntry();

            groupEntry.DisplayName = unifiedGroup.DisplayName;
            groupEntry.MailNickname = unifiedGroup.Name;
            groupEntry.IsPrivate = unifiedGroup.IsPrivate;

            groupEntry.Members = unifiedGroup.Members?.Select(m => new MemberEntry(m.Name)).ToList();
            groupEntry.Owners = unifiedGroup.Owners?.Select(m => new MemberEntry(m.Name)).ToList();

            return groupEntry;
        }
    }
}

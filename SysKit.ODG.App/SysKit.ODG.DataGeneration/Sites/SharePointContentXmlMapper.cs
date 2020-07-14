using System.Collections.Generic;
using System.Linq;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Enums;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.XmlTemplate.Model.Content;

namespace SysKit.ODG.Generation.Sites
{
    public class SharePointContentXmlMapper
    {
        public ContentEntry MapToContentEntry(XmlContent xmlContent, bool isRootWeb = false)
        {
            if (xmlContent == null)
            {
                return null;
            }

            validateContent(xmlContent, isRootWeb);
            var contentEntry = new ContentEntry(xmlContent.Name, xmlContent.Type)
            {
                CopyFromParent = xmlContent.CopyFromParent,
                HasUniqueRoleAssignments = xmlContent.IsUniqueRa,
                Children = xmlContent.Children?.Select(x => MapToContentEntry(x)).ToList() ?? new List<ContentEntry>()
            };

            if (xmlContent is XmlListItemContent xmlListItemContent)
            {
                contentEntry.SharingLinks = xmlListItemContent.SharingLinks?.Select(link => new SharingLinkEntry { IsEdit = link.IsEdit }).ToList() ?? new List<SharingLinkEntry>();
            }

            if (xmlContent.RoleAssignments?.Any() != true)
            {
                return contentEntry;
            }

            foreach (var roleAss in xmlContent.RoleAssignments)
            {
                if (!contentEntry.Assignments.ContainsKey(roleAss.Role))
                {
                    contentEntry.Assignments[roleAss.Role] = new HashSet<MemberEntry>();
                }

                if (roleAss.Members?.Any() != true)
                {
                    continue;
                }

                foreach (var member in roleAss.Members)
                {
                    if (string.IsNullOrEmpty(member.Name))
                    {
                        throw new XmlValidationException($"Member entry {member.Name} cannot be null.");
                    }

                    contentEntry.Assignments[roleAss.Role].Add(new MemberEntry(member.Name));
                }
            }

            return contentEntry;
        }

        private void validateContent(XmlContent xmlContent, bool isRootWeb)
        {
            // for root web we don't have name
            if (string.IsNullOrEmpty(xmlContent.Name) == !isRootWeb)
            {
                throw new XmlValidationException($"Content {nameof(xmlContent.Name)} property is not defined");
            }

            if (xmlContent.Type == ContentTypeEnum.None)
            {
                throw new XmlValidationException($"Content type is not recognized");
            }
        }
    }
}

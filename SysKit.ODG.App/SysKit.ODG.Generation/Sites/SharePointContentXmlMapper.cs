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
            return new ContentEntry(xmlContent.Name, xmlContent.Type)
            {
                Children = xmlContent.Children?.Select(x => MapToContentEntry(x)).ToList() ?? new List<ContentEntry>()
            };
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

using System.Xml.Serialization;

namespace SysKit.ODG.Base.XmlTemplate.Model.Content
{
    public class XmlListItemContent : XmlContent
    {
        /// <summary>
        /// Anonymous sharing links. For now only them because for other you need to open it
        /// </summary>
        [XmlArray]
        public XmlSharingLink[] SharingLinks { get; set; }
    }
}

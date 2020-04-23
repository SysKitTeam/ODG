namespace SysKit.ODG.Base.DTO.Generation
{
    public class UnifiedGroupEntry : GroupEntry, ISharePointContent
    {
        public string MailNickname { get; set; }
        public string SiteUrl { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsTeam { get; protected set; }

        public bool ProvisionFailed => string.IsNullOrEmpty(SiteUrl);

        public string Url => SiteUrl;
        public ContentEntry Content { get; set; }
    }
}

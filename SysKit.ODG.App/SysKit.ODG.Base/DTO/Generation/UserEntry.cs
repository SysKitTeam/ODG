namespace SysKit.ODG.Base.DTO.Generation
{
    public class UserEntry
    {
        public string Id { get; set; }
        public bool? AccountEnabled { get; set; }
        public string DisplayName { get; set; }
        public string MailNickname { get; set; }
        public string UserPrincipalName { get; set; }
        public string Password { get; set; }

        public bool? SetUserPhoto { get; set; }

    }
}

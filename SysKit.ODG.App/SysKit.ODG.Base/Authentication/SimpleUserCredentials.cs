using System.Security;

namespace SysKit.ODG.Base.Authentication
{
    public class SimpleUserCredentials
    {
        public string Username { get; }
        public SecureString Password { get; }

        public SimpleUserCredentials(string username, string password)
        {
            Username = username;
            Password = new SecureString();
            foreach (char c in password)
            {
                Password.AppendChar(c);
            }
        }
    }
}

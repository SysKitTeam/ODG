using SysKit.ODG.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Authentication
{
    public class AccessTokenManager: IAccessTokenManager
    {
        private readonly IAppConfigManager _appConfigManager;
        private IPublicClientApplication _app;
        private const string _authorityFormat = "https://login.microsoftonline.com/{0}/v2.0";

        public AccessTokenManager(IAppConfigManager appConfigManager)
        {
            _appConfigManager = appConfigManager;
            initializeApp();
        }

        private void initializeApp()
        {
            string authority = "https://login.microsoftonline.com/contoso.com";
            _app = PublicClientApplicationBuilder.Create(_appConfigManager.ClientId)
                .WithAuthority(authority)
                .Build();
        }

        public async Task<AuthToken> GetGraphToken(SimpleUserCredentials userCredentials)
        {
            var accounts = await _app.GetAccountsAsync();

            AuthenticationResult result;
            if (accounts.Any())
            {
                // TODO: what if more than one user is present???
                result = await _app.AcquireTokenSilent(_appConfigManager.Scopes, accounts.FirstOrDefault()).ExecuteAsync();
                return new AuthToken { Token = result.AccessToken };
            }

            try
            {

                result = await _app.AcquireTokenByUsernamePassword(_appConfigManager.Scopes,
                        userCredentials.Username,
                        userCredentials.Password)
                    .ExecuteAsync();
                return new AuthToken { Token = result.AccessToken };
            }
            catch (MsalUiRequiredException ex)
            {
                // TODO: HALP
                throw;
            }
            catch (Exception e)
            {
                // TODO: Logg exception
                throw;
            }
        }
    }
}

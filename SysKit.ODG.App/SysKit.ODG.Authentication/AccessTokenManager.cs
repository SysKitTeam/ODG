using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Authentication
{
    public class AccessTokenManager : IAccessTokenManager
    {
        private readonly IAppConfigManager _appConfigManager;
        private readonly SimpleUserCredentials _userCredentials;

        private IPublicClientApplication _app;
        private const string _authorityFormat = "https://login.microsoftonline.com/{0}/v2.0";
        // TODO: depending on registration this will maybe go back to IAppConfigManager
        private readonly string _clientId;

        public AccessTokenManager(IAppConfigManager appConfigManager, SimpleUserCredentials userCredentials, string clientId)
        {
            _appConfigManager = appConfigManager;
            _userCredentials = userCredentials;
            _clientId = clientId;
            initializeApp();
        }

        private void initializeApp()
        {
            string authority = "https://login.microsoftonline.com/organizations/oauth2/v2.0";
            _app = PublicClientApplicationBuilder.Create(_clientId)
                .WithAuthority(authority)
                .Build();
        }

        public async Task<AuthToken> GetGraphToken()
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
                        _userCredentials.Username,
                        _userCredentials.Password)
                    .ExecuteAsync();
                return new AuthToken { Token = result.AccessToken };
            }
            catch (MsalUiRequiredException)
            {
                // TODO: HALP
                throw;
            }
            catch (Exception)
            {
                // TODO: Logg exception
                throw;
            }
        }

        public async Task<AuthToken> GetSharePointToken()
        {
            var tenantName = _userCredentials.Username.Split('@')[1].Replace(".onmicrosoft.com", "");
            var sharePointScope = $"https://{tenantName}.sharepoint.com/AllSites.FullControl";

            try
            {
                var result = await _app.AcquireTokenByUsernamePassword(new[] { sharePointScope },
                        _userCredentials.Username,
                        _userCredentials.Password)
                    .ExecuteAsync();
                return new AuthToken { Token = result.AccessToken };
            }
            catch (MsalUiRequiredException)
            {
                // TODO: HALP
                throw;
            }
            catch (Exception)
            {
                // TODO: Logg exception
                throw;
            }
        }

        public string GetUsernameFromToken()
        {
            return _userCredentials.Username;
        }
    }
}

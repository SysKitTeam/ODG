using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Graph;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;
using SysKit.ODG.Base.Office365;
using SysKit.ODG.Office365Service.GraphHttpProvider;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public class UserGraphApiClient: BaseGraphApiClient, IUserGraphApiClient
    {
        private readonly INotifier _notifier;
        public UserGraphApiClient(IAccessTokenManager accessTokenManager,
            INotifier notifier,
            IGraphHttpProviderFactory graphHttpProviderFactory,
            IGraphServiceFactory graphServiceFactory,
            IMapper autoMapper) : base(accessTokenManager, graphHttpProviderFactory, graphServiceFactory, autoMapper)
        {
            _notifier = notifier;
        }

        /// <inheritdoc />
        public async Task<UserEntryCollection> GetAllTenantUsers(string tenantDomain)
        {
            var userRequest = _graphServiceClient.Users.Request().Top(999);
            var userEntries = new List<UserEntry>();

            do
            {
                var users = await userRequest.GetAsync();
                foreach (var user in users)
                {
                    userEntries.Add(_autoMapper.Map<User, UserEntry>(user));
                }

                userRequest = users.NextPageRequest;
            } while (userRequest != null);

            return new UserEntryCollection(tenantDomain, userEntries);
        }

        /// <inheritdoc />
        public async Task<List<UserEntry>> CreateTenantUsers(IEnumerable<UserEntry> users)
        {
            var userLookup = new Dictionary<string, UserEntry>();
            var successfullyCreatedUsers = new ConcurrentBag<UserEntry>();
            var batchEntries = new List<GraphBatchRequest>();
            foreach (var user in users)
            {
                if (userLookup.ContainsKey(user.UserPrincipalName))
                {
                    _notifier.Warning(new NotifyEntry("Create Users", $"Trying to create user with same name ({user.UserPrincipalName}), only the first one will be created"));
                }

                userLookup.Add(user.UserPrincipalName, user);
                var graphUser = _autoMapper.Map<UserEntry, User>(user, config => config.AfterMap((src, dest) =>
                {
                    dest.PasswordProfile = new PasswordProfile
                    {
                        Password = src.Password,
                        ForceChangePasswordNextSignIn = false
                    };
                }));

                batchEntries.Add(new GraphBatchRequest(graphUser.UserPrincipalName, "users", HttpMethod.Post, graphUser));
            }

            if (!batchEntries.Any())
            {
                return new List<UserEntry>();
            }

            var progressUpdater = new ProgressUpdater(batchEntries.Count, "Create Users", _notifier);
            Action<Dictionary<string, HttpResponseMessage>> handleBatchResult = results =>
            {
                foreach (var result in results)
                {
                    var originalUser = userLookup[result.Key];
                    if (result.Value.IsSuccessStatusCode)
                    {
                        var graphUser = deserializeGraphObject<User>(result.Value.Content).GetAwaiter().GetResult();
                        originalUser.Id = graphUser.Id;
                        graphUser.UserPrincipalName = graphUser.UserPrincipalName;
                        successfullyCreatedUsers.Add(originalUser);
                    }
                    else
                    {
                        _notifier.Error(new NotifyEntry("Create Users", $"Failed to create: {originalUser.UserPrincipalName}. {getErrorMessage(result.Value)}"));
                    }

                    result.Value.Dispose();
                }

                progressUpdater.UpdateProgress(results.Count);
            };

            await _httpProvider.StreamBatchAsync(batchEntries, _accessTokenManager, handleBatchResult);

            return successfullyCreatedUsers.ToList();
        }

    }
}

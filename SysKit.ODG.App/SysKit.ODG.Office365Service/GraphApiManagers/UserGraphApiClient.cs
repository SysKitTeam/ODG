using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;
using SysKit.ODG.Base.Office365;
using SysKit.ODG.Office365Service.GraphHttpProvider;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public class UserGraphApiClient : BaseGraphApiClient, IUserGraphApiClient
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
        public async Task<O365CreationResult<UserEntry>> CreateTenantUsers(IEnumerable<UserEntry> users)
        {
            using var progressUpdater = new ProgressUpdater("Create Users", _notifier);
            var userLookup = new Dictionary<string, UserEntry>();
            var successfullyCreatedUsers = new ConcurrentBag<UserEntry>();
            var batchEntries = new List<GraphBatchRequest>();
            var hadErrors = false;

            foreach (var user in users)
            {
                if (userLookup.ContainsKey(user.UserPrincipalName))
                {
                    _notifier.Warning($"Trying to create user with same name ({user.UserPrincipalName}), only the first one will be created");
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

            // there is a limit of around 5k users every 5-10min
            var maxConcurrentRequests = batchEntries.Count > 4000 ? 2 : 6;
            await executeActionWithProgress(progressUpdater, batchEntries, maxConcurrentRequests: maxConcurrentRequests, onResult: (key, value) =>
            {
                var originalUser = userLookup[key];
                if (value.IsSuccessStatusCode)
                {
                    var graphUser = deserializeGraphObject<User>(value.Content).GetAwaiter().GetResult();
                    originalUser.Id = graphUser.Id;
                    graphUser.UserPrincipalName = graphUser.UserPrincipalName;
                    successfullyCreatedUsers.Add(originalUser);
                }
                else
                {
                    if (isKnownError(GraphAPIKnownErrorMessages.UserAlreadyExists, value))
                    {
                        _notifier.Warning($"Failed to create: {originalUser.UserPrincipalName}. User already exists.");
                    }
                    else
                    {
                        _notifier.Error($"Failed to create: {originalUser.UserPrincipalName}. {getErrorMessage(value)}");
                        hadErrors = true;
                    }
                }
            });

            return new O365CreationResult<UserEntry>(successfullyCreatedUsers, hadErrors);
        }

        /// <inheritdoc />
        public async Task<bool> CreateUserManagers(
            List<ManagerSubordinatePair> managerSubordinatePairs)
        {
            using var progressUpdater = new ProgressUpdater("Assign Managers", _notifier);
            var batchEntries = new List<GraphBatchRequest>();
            var hadErrors = false;

            foreach (var managerSubordinatePair in managerSubordinatePairs)
            {
                var managerProp = new JProperty("@odata.id", $"https://graph.microsoft.com/v1.0/users/{managerSubordinatePair.ManagerGuid}");
                var body = new JObject { managerProp };

                batchEntries.Add(new GraphBatchRequest(managerSubordinatePair.SubordinateGuid, $"/users/{managerSubordinatePair.SubordinateGuid}/manager/$ref", HttpMethod.Put, body.ToString()));
            }

            await executeActionWithProgress(progressUpdater, batchEntries, onResult: (key, value) =>
            {
                if (!value.IsSuccessStatusCode)
                {
                    _notifier.Error($"Failed to create manager for: {key}. {getErrorMessage(value)}");
                    hadErrors = true;
                }
            });

            return hadErrors;

        }
    }
}

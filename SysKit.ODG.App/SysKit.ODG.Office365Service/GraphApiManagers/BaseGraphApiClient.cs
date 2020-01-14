using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Office365Service.GraphHttpProvider;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public abstract class BaseGraphApiClient
    {
        protected readonly IMapper _autoMapper;
        /// <summary>
        /// Used to ping v1 Graph API
        /// </summary>
        protected readonly IGraphServiceClient _graphServiceClient;
        /// <summary>
        /// Used to ping beta endpoint
        /// </summary>
        protected readonly IGraphServiceClient _graphServiceBetaClient;
        protected readonly IGraphHttpProvider _httpProvider;

        protected readonly IAccessTokenManager _accessTokenManager;

        protected BaseGraphApiClient(IAccessTokenManager accessTokenManager,
            IGraphHttpProviderFactory graphHttpProviderFactory,
            IGraphServiceFactory graphServiceFactory, 
            IMapper autoMapper)
        {
            _graphServiceClient = graphServiceFactory.CreateGraphServiceClient(accessTokenManager, false);
            _graphServiceBetaClient = graphServiceFactory.CreateGraphServiceClient(accessTokenManager, true);
            _autoMapper = autoMapper;
            _httpProvider = graphHttpProviderFactory.CreateHttpProvider();
            _accessTokenManager = accessTokenManager;
        }

        #region Request execution helpers

        /// <summary>
        /// Helper for deserializing Graph API objects
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        protected async Task<TResult> deserializeGraphObject<TResult>(HttpContent content)
        {
            return _graphServiceClient.HttpProvider.Serializer.DeserializeObject<TResult>(await content.ReadAsStreamAsync());
        }

        /// <summary>
        /// Extracts error message and status code from response
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        protected string getErrorMessage(HttpResponseMessage responseMessage)
        {
            var content = responseMessage.Content != null ? responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult() : null;
            return $"Status code: {responseMessage.StatusCode}; Error: {content}";
        }
       
        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;
using SysKit.ODG.Office365Service.GraphHttpProvider;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;

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

        /// <summary>
        /// Determine if it is known error from error message
        /// </summary>
        /// <param name="expectedMessage"></param>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        protected bool isKnownError(string expectedMessage, HttpResponseMessage responseMessage)
        {
            if (responseMessage.Content == null)
            {
                return expectedMessage == null;
            }

            var content = responseMessage.Content.ReadAsAsync<GraphApiError>().GetAwaiter().GetResult();
            return content?.Error?.Message?.Contains(expectedMessage) == true;
        }

        /// <summary>
        /// Determine if it is known error from error status code
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        protected bool isKnownError(HttpStatusCode statusCode, HttpResponseMessage responseMessage)
        {
            return responseMessage.StatusCode == statusCode;
        }

        protected bool isKnownError(string expectedMessage, Error error)
        {
            if (error?.Message == null)
            {
                return expectedMessage == null;
            }

            return error.Message.Contains(expectedMessage);
        }

        /// <summary>
        /// Executes request with progress update
        /// </summary>
        /// <param name="progressUpdater"></param>
        /// <param name="batchEntries"></param>
        /// <param name="useBetaEndpoint"></param>
        /// <param name="onResult"></param>
        /// <returns></returns>
        protected async Task executeActionWithProgress(ProgressUpdater progressUpdater, List<GraphBatchRequest> batchEntries, bool useBetaEndpoint = false, Action<string, HttpResponseMessage> onResult = null, int maxConcurrentRequests = 3)
        {
            if (!batchEntries.Any())
            {
                return;
            }

            progressUpdater.SetTotalCount(batchEntries.Count);
            Action<Dictionary<string, HttpResponseMessage>> handleBatchResult = results =>
            {
                foreach (var result in results)
                {
                    onResult?.Invoke(result.Key, result.Value);
                    result.Value.Dispose();
                }

                progressUpdater.UpdateProgress(results.Count);
            };

            await _httpProvider.StreamBatchAsync(batchEntries, _accessTokenManager, handleBatchResult, useBetaEndpoint, maxConcurrentRequests);
            progressUpdater.Flush();
        }
        #endregion

        class GraphApiError
        {
            public Error Error { get; set; }
        }
    }
}

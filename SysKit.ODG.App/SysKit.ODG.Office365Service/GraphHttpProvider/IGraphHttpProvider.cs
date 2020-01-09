using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    public interface IGraphHttpProvider: IHttpProvider
    {
        /// <summary>
        /// Executes batch requests and returns all results
        /// </summary>
        /// <param name="batchEntries"></param>
        /// <param name="tokenRetriever"></param>
        /// <param name="useBetaEndpoint"></param>
        /// <param name="maxConcurrent"></param>
        /// <returns></returns>
        Task<Dictionary<string, HttpResponseMessage>> SendBatchAsync(IEnumerable<GraphBatchRequest> batchEntries, IAccessTokenManager tokenRetriever, bool useBetaEndpoint = false, int maxConcurrent = 8);

        /// <summary>
        /// Similar to SendBatchAsync, but returns batches as they are completed (and not in the end)
        /// </summary>
        /// <param name="batchEntries"></param>
        /// <param name="tokenRetriever"></param>
        /// <param name="handleBatchResult">Method for handling batch responses</param>
        /// <param name="useBetaEndpoint"></param>
        /// <param name="maxConcurrent"></param>
        /// <returns></returns>
        Task StreamBatchAsync(IEnumerable<GraphBatchRequest> batchEntries, IAccessTokenManager tokenRetriever,
            Action<Dictionary<string, HttpResponseMessage>> handleBatchResult, bool useBetaEndpoint = false,
            int maxConcurrent = 8);
    }
}

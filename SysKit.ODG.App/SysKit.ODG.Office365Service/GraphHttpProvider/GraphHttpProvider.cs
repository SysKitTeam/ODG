using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Newtonsoft.Json;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    /// <summary>
    /// Custom implementation of HttpProvider that handles throttling
    /// </summary>
    public class GraphHttpProvider: HttpProvider, IGraphHttpProvider
    {
        public GraphHttpProvider(): this(new HttpClientHandler())
        {

        }

        public GraphHttpProvider(HttpMessageHandler innerHandler): base(innerHandler, false, null)
        {
        }

        public async Task<IEnumerable<HttpResponseMessage>> SendBatchAsync(IEnumerable<GraphBatchRequest> batchEntries, string token, bool useBetaEndpoint = false)
        {
            var endpoint = useBetaEndpoint ? "beta" : "v1.0";
            Func<string, string> createUrl = relativeUrl => $"https://graph.microsoft.com/{endpoint}/{relativeUrl}";

            var allRequests = batchEntries.ToList();
            var maxRequestCountPerBatch = 20;
            var page = 0;

            var batchResults = new List<HttpResponseMessage>();

            List<GraphBatchRequest> tmpRequests = allRequests.Skip(page * maxRequestCountPerBatch).Take(maxRequestCountPerBatch).ToList();

            do
            {
                var requestsToExecute = tmpRequests;
                var batch = new BatchRequestContent();

                foreach (var entry in requestsToExecute)
                {
                    var httpRequestMessage = new HttpRequestMessage(entry.Method, createUrl(entry.RelativeUrl));

                    if (entry.Content != null)
                    {
                        httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(entry.Content), Encoding.UTF8, "application/json");
                    }

                    //httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
                    //    _accessTokenManager.GetGraphToken().GetAwaiter().GetResult().Token);

                    var batchStep = new BatchRequestStep(entry.Id, httpRequestMessage);
                    batch.AddBatchRequestStep(batchStep);
                }

                // Send batch request with BatchRequestContent.
                var batchHttpRequest = new HttpRequestMessage(HttpMethod.Post,createUrl("$batch")) { Content = batch };
                batchHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage batchRequest = await SendAsync(batchHttpRequest);

                var batchResponseContent = new BatchResponseContent(batchRequest);
                var batchResponses = await batchResponseContent.GetResponsesAsync();
                foreach (var response in batchResponses)
                {
                    batchResults.Add(response.Value);
                }

                page++;
                tmpRequests = allRequests.Skip(page * maxRequestCountPerBatch).Take(maxRequestCountPerBatch).ToList();
            } while (tmpRequests.Any());

            return batchResults;
        }

        //private async Task<HttpResponseMessage> sendInternal(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken, int retryCount)
        //{
        //    HttpResponseMessage response = null;

        //    try
        //    {
        //        if (!string.IsNullOrEmpty(_userAgent))
        //        {
        //            request.Headers.UserAgent.Clear();
        //            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(_userAgent));
        //        }

        //        response = await base.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
        //        return response;
        //    }
        //    catch (ServiceException sex)
        //    {
        //        // handle throttling
        //        if (((int)sex.StatusCode == 429 || (int)sex.StatusCode == 503) && retryCount > 0)
        //        {
        //            var retryAfter = ThrottleUtil.GetRetryAfterValue(response.Headers);
        //            var urlData = $"Host: {request.RequestUri.Host}, URL: {request.RequestUri.AbsolutePath}, Remaining tries: {retryCount}";
        //            //EventLogManager.WriteEntry($"Requests have been throttled! retrying after {retryAfter / 1000} seconds. {urlData}", EventLogEntryType.Warning, TracingLevelEnum.Verbose);

        //            await Task.Delay(retryAfter, cancellationToken);
        //            return await sendInternal(request, completionOption, cancellationToken, retryCount - 1);
        //        }

        //        if (((int)sex.StatusCode == 429 || (int)sex.StatusCode == 503) && retryCount == 0)
        //        {
        //            var urlData = $"Host: {request.RequestUri.Host}, URL: {request.RequestUri.AbsolutePath}, Remaining tries: {retryCount}";
        //            //EventLogManager.WriteEntry($"Requests have been throttled too many times! Request failed! {urlData}", EventLogEntryType.Warning);
        //            // TODO: max retry exception
        //            throw;
        //        }

        //        throw;
        //    }
        //}
    }
}

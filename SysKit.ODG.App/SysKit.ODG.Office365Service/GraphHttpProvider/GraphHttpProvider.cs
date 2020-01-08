using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Newtonsoft.Json;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;
using SysKit.ODG.Office365Service.Polly;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    /// <summary>
    /// Custom implementation of HttpProvider that handles throttling
    /// </summary>
    public class GraphHttpProvider: HttpProvider, IGraphHttpProvider
    {
        private ICustomRetryPolicy _customRetryPolicy;
        public GraphHttpProvider(HttpMessageHandler innerHandler, ICustomRetryPolicy customRetryPolicy) : base(innerHandler, false, null)
        {
            _customRetryPolicy = customRetryPolicy;
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
                var requestsToExecute = tmpRequests.ToDictionary(x => x.Id, x => x);
                batchResults.AddRange(await sendBatchAsync(token, requestsToExecute, createUrl));
                page++;
                tmpRequests = allRequests.Skip(page * maxRequestCountPerBatch).Take(maxRequestCountPerBatch).ToList();
            } while (tmpRequests.Any());

            return batchResults;
        }

        private async Task<IEnumerable<HttpResponseMessage>> sendBatchAsync(string token, Dictionary<string, GraphBatchRequest> requestsToExecute, Func<string, string> createUrl)
        {
            var batchResults = new List<HttpResponseMessage>();
            // copy because some requests can fail
            var tmpRequestsToExecute = requestsToExecute;

            await _customRetryPolicy.ExecuteAsync(async () =>
            {
                var isThrottled = false;
                var retryAfterValue = TimeSpan.Zero;

                var batchHttpRequest = createRequestMessage(token, tmpRequestsToExecute, createUrl);
                // we will add all failed requests here to try them again
                tmpRequestsToExecute = new Dictionary<string, GraphBatchRequest>();
                HttpResponseMessage batchRequest = await SendAsync(batchHttpRequest);

                var batchResponseContent = new BatchResponseContent(batchRequest);
                var batchResponses = await batchResponseContent.GetResponsesAsync();
                foreach (var response in batchResponses)
                {
                    if (_customRetryPolicy.IsResponseThrottled(response.Value))
                    {
                        isThrottled = true;
                        var headerRetryValue =
                            _customRetryPolicy.GetRetryAfterValueFromResponseHeader(response.Value.Headers) ??
                            TimeSpan.Zero;

                        retryAfterValue = retryAfterValue > headerRetryValue ? retryAfterValue : headerRetryValue;
                        // we need to execute it again
                        tmpRequestsToExecute.Add(response.Key, requestsToExecute[response.Key]);
                    }
                    else
                    {
                        batchResults.Add(response.Value);
                    }
                }

                if (isThrottled)
                {
                    throw new ThrottleException(retryAfterValue);
                }

            }, createUrl("$batch"));

            return batchResults;
        }

        private HttpRequestMessage createRequestMessage(string token, Dictionary<string, GraphBatchRequest> requestsToExecute, Func<string, string> createUrl)
        {
            var batch = new BatchRequestContent();

            foreach (var entry in requestsToExecute)
            {
                var httpRequestMessage = new HttpRequestMessage(entry.Value.Method, createUrl(entry.Value.RelativeUrl));

                if (entry.Value.Content != null)
                {
                    httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(entry.Value.Content), Encoding.UTF8,
                        "application/json");
                }

                var batchStep = new BatchRequestStep(entry.Value.Id, httpRequestMessage);
                batch.AddBatchRequestStep(batchStep);
            }

            // Send batch request with BatchRequestContent.
            var batchHttpRequest = new HttpRequestMessage(HttpMethod.Post, createUrl("$batch")) {Content = batch};
            batchHttpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return batchHttpRequest;
        }
    }
}

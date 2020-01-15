using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Graph;
using Newtonsoft.Json;
using OfficeDevPnP.Core.Extensions;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.Interfaces.Authentication;
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

        /// <inheritdoc />
        public async Task<Dictionary<string, HttpResponseMessage>> SendBatchAsync(IEnumerable<GraphBatchRequest> batchEntries, IAccessTokenManager tokenRetriever, bool useBetaEndpoint = false, int maxConcurrent = 3)
        {
            var results = new ConcurrentDictionary<string, HttpResponseMessage>();
            
            Action<Dictionary<string, HttpResponseMessage>> handleBatchResult = responses =>
            {
                foreach (var response in responses)
                {
                    results.TryAdd(response.Key, response.Value);
                }
            };

            await Task.WhenAll(StreamBatchAsync(batchEntries, tokenRetriever, handleBatchResult, useBetaEndpoint, maxConcurrent));

            return results.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <inheritdoc />
        public async Task StreamBatchAsync(IEnumerable<GraphBatchRequest> batchEntries, IAccessTokenManager tokenRetriever, Action<Dictionary<string, HttpResponseMessage>> handleBatchResult, bool useBetaEndpoint = false, int maxConcurrent = 3)
        {
            var endpoint = useBetaEndpoint ? "beta" : "v1.0";
            Func<string, string> createUrl = relativeUrl => $"https://graph.microsoft.com/{endpoint}/{relativeUrl}";

            var allRequests = batchEntries.ToList();
            var maxRequestCountPerBatch = 20;
            var page = 0;

            List<GraphBatchRequest> tmpRequests =
                allRequests.Skip(page * maxRequestCountPerBatch).Take(maxRequestCountPerBatch).ToList();

            var batchesToProcess = new BufferBlock<Dictionary<string, GraphBatchRequest>>();
            var executeRequestsBlock = new TransformBlock<Dictionary<string, GraphBatchRequest>, Dictionary<string, HttpResponseMessage>>(batches => execute(tokenRetriever, batches, createUrl), new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxConcurrent
            });

            var finalBlock = new ActionBlock<Dictionary<string, HttpResponseMessage>>(handleBatchResult);

            // link blocks: batches => execute them => save results
            batchesToProcess.LinkTo(executeRequestsBlock, new DataflowLinkOptions { PropagateCompletion = true });
            executeRequestsBlock.LinkTo(finalBlock, new DataflowLinkOptions { PropagateCompletion = true });

            while (tmpRequests.Any())
            {
                batchesToProcess.Post(tmpRequests.ToDictionary(x => x.Id, x => x));
                page++;
                tmpRequests = allRequests.Skip(page * maxRequestCountPerBatch).Take(maxRequestCountPerBatch).ToList();
            }

            batchesToProcess.Complete();

            await Task.WhenAll(executeRequestsBlock.Completion, finalBlock.Completion);
        }

        private async Task<Dictionary<string, HttpResponseMessage>> execute(IAccessTokenManager tokenRetriever, Dictionary<string, GraphBatchRequest> requestsToExecute, Func<string, string> createUrl)
        {
            var batchResults = new Dictionary<string, HttpResponseMessage>();
            // copy because some requests can fail
            var tmpRequestsToExecute = requestsToExecute;

            await _customRetryPolicy.ExecuteAsync(async () =>
            {
                var token = await tokenRetriever.GetGraphToken();
                var isThrottled = false;
                var retryAfterValue = TimeSpan.Zero;

                var batchHttpRequest = createRequestMessage(token.Token, tmpRequestsToExecute, createUrl);
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
                        batchResults.Add(response.Key, response.Value);
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

                if (entry.Value.Content is string jsonContent)
                {
                    httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8,
                        "application/json");
                }
                else if (entry.Value.Content != null)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using OfficeDevPnP.Core.Diagnostics;

namespace SysKit.ODG.Office365Service.Utils
{
    /// <summary>
    /// Custom implementation of HttpProvider that handles throttling
    /// </summary>
    public class CustomHttpProvider: HttpProvider, IHttpProvider
    {
        private readonly int _retryCount;
        private readonly string _userAgent;

        public CustomHttpProvider(int retryCount, string userAgent = null)
        {
            _retryCount = retryCount;
            _userAgent = userAgent;
        }

        Task<HttpResponseMessage> IHttpProvider.SendAsync(HttpRequestMessage request)
        {
            return sendInternal(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None, _retryCount);
        }

        Task<HttpResponseMessage> IHttpProvider.SendAsync(
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            return sendInternal(request, completionOption, cancellationToken, _retryCount);
        }

        private async Task<HttpResponseMessage> sendInternal(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken, int retryCount)
        {
            HttpResponseMessage response = null;

            try
            {
                if (!string.IsNullOrEmpty(_userAgent))
                {
                    request.Headers.UserAgent.Clear();
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue(_userAgent));
                }

                response = await base.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
                return response;
            }
            catch (ServiceException sex)
            {
                // handle throttling
                if (((int)sex.StatusCode == 429 || (int)sex.StatusCode == 503) && retryCount > 0)
                {
                    var retryAfter = ThrottleUtil.GetRetryAfterValue(response.Headers);
                    var urlData = $"Host: {request.RequestUri.Host}, URL: {request.RequestUri.AbsolutePath}, Remaining tries: {retryCount}";
                    //EventLogManager.WriteEntry($"Requests have been throttled! retrying after {retryAfter / 1000} seconds. {urlData}", EventLogEntryType.Warning, TracingLevelEnum.Verbose);

                    await Task.Delay(retryAfter, cancellationToken);
                    return await sendInternal(request, completionOption, cancellationToken, retryCount - 1);
                }

                if (((int)sex.StatusCode == 429 || (int)sex.StatusCode == 503) && retryCount == 0)
                {
                    var urlData = $"Host: {request.RequestUri.Host}, URL: {request.RequestUri.AbsolutePath}, Remaining tries: {retryCount}";
                    //EventLogManager.WriteEntry($"Requests have been throttled too many times! Request failed! {urlData}", EventLogEntryType.Warning);
                    // TODO: max retry exception
                    throw;
                }

                throw;
            }
        }
    }
}

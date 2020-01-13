using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SysKit.ODG.Office365Service.GraphHttpProvider.Handlers
{
    /// <summary>
    /// Used to set User Agent for each request
    /// </summary>
    public class UserAgentHandler: DelegatingHandler
    {
        private readonly string _userAgent;
        public UserAgentHandler(string userAgent)
        {
            _userAgent = userAgent;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            request.Headers.UserAgent.Clear();
            request.Headers.Add("User-Agent", _userAgent);
            return base.SendAsync(request, token);
        }
    }
}

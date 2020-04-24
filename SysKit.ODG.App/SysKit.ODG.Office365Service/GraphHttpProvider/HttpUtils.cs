using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    public static class HttpUtils
    {
        /// <summary>
        /// Creates a request to be used with HttpProvider
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="tokenManager"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task<HttpRequestMessage> CreateRequest(string url, HttpMethod method, IAccessTokenManager tokenManager, object content = null)
        {
            var httpRequestMessage = new HttpRequestMessage(method, url);

            if (content is string jsonContent)
            {
                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8,
                    "application/json");
            }
            else if (content != null)
            {
                httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8,
                    "application/json");
            }

            var token = await tokenManager.GetGraphToken();
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            return httpRequestMessage;
        }

        /// <summary>
        /// Creates a full URL for GraphAPI endpoint from relative (EXAMPLE: /teams -> https://graph.microsoft.com/v1.0/teams)
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <param name="isBetaEndpoint"></param>
        /// <returns></returns>
        public static string CreateGraphUrl(string relativeUrl, bool isBetaEndpoint = false)
        {
            var endpoint = isBetaEndpoint ? "beta" : "v1.0";
            return $"https://graph.microsoft.com/{endpoint}/{relativeUrl.TrimStart('/')}";
        }
    }
}

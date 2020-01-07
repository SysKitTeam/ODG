using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    public interface IGraphHttpProviderFactory
    {
        /// <summary>
        /// Used to create custom HttpProvider used for GraphClientService and custom Http requests to Graph API
        /// </summary>
        /// <param name="finalHandler">If not set HttpClientHandler will be used</param>
        /// <returns></returns>
        IGraphHttpProvider CreateHttpProvider(HttpMessageHandler finalHandler = null);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    public interface IGraphHttpProvider: IHttpProvider
    {
        Task<IEnumerable<HttpResponseMessage>> SendBatchAsync(IEnumerable<GraphBatchRequest> batchEntries, string token, bool useBetaEndpoint = false);
    }
}

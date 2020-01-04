using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using SysKit.ODG.Office365Service.GraphHttpProvider.Dto;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    public interface IGraphHttpProvider: IHttpProvider
    {
        Task<BatchResponseContent> SendBatchAsync(IEnumerable<GraphBatchEntry> batchEntries, string token);
    }
}

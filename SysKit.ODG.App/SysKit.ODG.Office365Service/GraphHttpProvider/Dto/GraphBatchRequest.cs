using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SysKit.ODG.Office365Service.GraphHttpProvider.Dto
{
    public class GraphBatchRequest
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public HttpMethod Method { get; set; }
        public object Content { get; set; }

        public GraphBatchRequest(string id, string url): this(id, url, HttpMethod.Get)
        {

        }

        public GraphBatchRequest(string id, string url, HttpMethod method, object content = null)
        {
            Id = id;
            Url = url;
            Method = method;
            Content = content;
        }
    }
}

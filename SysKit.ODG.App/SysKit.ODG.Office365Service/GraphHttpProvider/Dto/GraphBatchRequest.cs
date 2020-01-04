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
        public string RelativeUrl { get; set; }
        public HttpMethod Method { get; set; }
        public object Content { get; set; }

        public GraphBatchRequest(string id, string relativeUrl) : this(id, relativeUrl, HttpMethod.Get)
        {

        }

        public GraphBatchRequest(string id, string relativeUrl, HttpMethod method, object content = null)
        {
            Id = id;
            RelativeUrl = relativeUrl.TrimStart('/');
            Method = method;
            Content = content;
        }
    }
}

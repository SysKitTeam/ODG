using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SysKit.ODG.Office365Service.Polly
{
    public interface ICustomRetryPolicy
    {
        Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> requestFunction);
    }
}

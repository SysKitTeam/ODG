using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysKit.ODG.Office365Service.GraphHttpProvider
{
    public interface IGraphHttpProviderFactory
    {
        IGraphHttpProvider CreateHttpProvider();
    }
}

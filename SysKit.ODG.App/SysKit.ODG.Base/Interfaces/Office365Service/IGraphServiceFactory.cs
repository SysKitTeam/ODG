using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Base.Interfaces.Office365Service
{
    public interface IGraphServiceFactory
    {
        /// <summary>
        /// Creates service client that is used for pinging Graph API
        /// </summary>
        /// <param name="accessTokenManager"></param>
        /// <param name="useBetaEndpoint"></param>
        /// <returns></returns>
        IGraphServiceClient CreateGraphServiceClient(IAccessTokenManager accessTokenManager, bool useBetaEndpoint = false);
    }
}

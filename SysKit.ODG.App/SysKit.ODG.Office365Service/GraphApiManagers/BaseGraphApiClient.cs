using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Office365Service.GraphHttpProvider;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public abstract class BaseGraphApiClient
    {
        protected readonly IMapper _autoMapper;
        /// <summary>
        /// Used to ping v1 Graph API
        /// </summary>
        protected readonly IGraphServiceClient _graphServiceClient;
        /// <summary>
        /// Used to ping beta endpoint
        /// </summary>
        protected readonly IGraphServiceClient _graphServiceBetaClient;
        protected readonly IGraphHttpProvider _httpProvider;

        protected readonly IAccessTokenManager _accessTokenManager;

        protected BaseGraphApiClient(IAccessTokenManager accessTokenManager,
            IGraphHttpProviderFactory graphHttpProviderFactory,
            IGraphServiceFactory graphServiceFactory, 
            IMapper autoMapper)
        {
            _graphServiceClient = graphServiceFactory.CreateGraphServiceClient(accessTokenManager, false);
            _graphServiceBetaClient = graphServiceFactory.CreateGraphServiceClient(accessTokenManager, true);
            _autoMapper = autoMapper;
            _httpProvider = graphHttpProviderFactory.CreateHttpProvider();
            _accessTokenManager = accessTokenManager;
        }

        #region Request execution helpers

       
        #endregion

    }
}

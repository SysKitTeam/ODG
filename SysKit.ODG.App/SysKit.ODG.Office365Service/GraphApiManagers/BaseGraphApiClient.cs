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
using SysKit.ODG.Office365Service.GraphHttpProvider;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public abstract class BaseGraphApiClient
    {
        protected readonly IAppConfigManager _appConfigManager;
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

        protected BaseGraphApiClient(IAppConfigManager appConfigManager, IAccessTokenManager accessTokenManager, IGraphHttpProvider httpProvider, IGraphServiceCreator graphServiceCreator, IMapper autoMapper)
        {
            _graphServiceClient = graphServiceCreator.CreateGraphServiceClient(accessTokenManager, false);
            _graphServiceBetaClient = graphServiceCreator.CreateGraphServiceClient(accessTokenManager, true);
            _autoMapper = autoMapper;
            _httpProvider = httpProvider;
            _accessTokenManager = accessTokenManager;
        }

        #region Request execution helpers

       
        #endregion

    }
}

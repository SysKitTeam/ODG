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

        protected BaseGraphApiClient(IAppConfigManager appConfigManager, IGraphServiceCreator graphServiceCreator, IMapper autoMapper)
        {
            _graphServiceClient = graphServiceCreator.CreateGraphServiceClient(false);
            _graphServiceBetaClient = graphServiceCreator.CreateGraphServiceClient(true);
            _autoMapper = autoMapper;
        }

        #region Request execution helpers

       
        #endregion

    }
}

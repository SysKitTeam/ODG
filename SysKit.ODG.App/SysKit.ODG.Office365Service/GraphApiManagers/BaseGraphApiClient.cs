using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using SysKit.ODG.Base.Interfaces.Authentication;

namespace SysKit.ODG.Office365Service.GraphApiManagers
{
    public abstract class BaseGraphApiClient
    {
        /// <summary>
        /// Used to ping v1 Graph API
        /// </summary>
        protected readonly IGraphServiceClient _graphServiceClient;
        /// <summary>
        /// Used to ping beta endpoint
        /// </summary>
        protected readonly IGraphServiceClient _graphServiceBetaClient;

        protected BaseGraphApiClient(IGraphServiceCreator graphServiceCreator)
        {
            _graphServiceClient = graphServiceCreator.CreateGraphServiceClient(false);
            _graphServiceBetaClient = graphServiceCreator.CreateGraphServiceClient(true);
        }

        #region Request execution helpers

       
        #endregion

    }
}

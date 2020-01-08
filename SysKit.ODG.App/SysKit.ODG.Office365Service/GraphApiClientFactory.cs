using AutoMapper;
using Serilog;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Office365Service.GraphApiManagers;
using SysKit.ODG.Office365Service.GraphHttpProvider;

namespace SysKit.ODG.Office365Service
{
    public class GraphApiClientFactory : IGraphApiClientFactory
    {
        private readonly IGraphHttpProviderFactory _graphHttpProviderFactory;
        private readonly IGraphServiceFactory _graphServiceFactory;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public GraphApiClientFactory(ILogger logger,
            IGraphHttpProviderFactory graphHttpProviderFactory,
            IGraphServiceFactory graphServiceFactory,
            IMapper mapper)
        {
            _graphHttpProviderFactory = graphHttpProviderFactory;
            _graphServiceFactory = graphServiceFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public IUserGraphApiClient CreateUserGraphApiClient(IAccessTokenManager accessTokenManager)
        {
            return new UserGraphApiClient(accessTokenManager, _logger, _graphHttpProviderFactory, _graphServiceFactory, _mapper);
        }

        public IGroupGraphApiClient CreateGroupGraphApiClient(IAccessTokenManager accessTokenManager)
        {
            return new GroupGraphApiClient(accessTokenManager, _logger, _graphHttpProviderFactory, _graphServiceFactory, _mapper);
        }
    }
}

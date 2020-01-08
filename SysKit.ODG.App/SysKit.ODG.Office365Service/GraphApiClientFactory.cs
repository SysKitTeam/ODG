using AutoMapper;
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

        public GraphApiClientFactory(IGraphHttpProviderFactory graphHttpProviderFactory,
            IGraphServiceFactory graphServiceFactory,
            IMapper mapper)
        {
            _graphHttpProviderFactory = graphHttpProviderFactory;
            _graphServiceFactory = graphServiceFactory;
            _mapper = mapper;
        }

        public IUserGraphApiClient CreateUserGraphApiClient(IAccessTokenManager accessTokenManager)
        {
            return new UserGraphApiClient(accessTokenManager, _graphHttpProviderFactory, _graphServiceFactory, _mapper);
        }
    }
}

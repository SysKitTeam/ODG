using AutoMapper;
using Serilog;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;
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

        public IUserGraphApiClient CreateUserGraphApiClient(IAccessTokenManager accessTokenManager, INotifier notifier)
        {
            return new UserGraphApiClient(accessTokenManager, notifier, _graphHttpProviderFactory, _graphServiceFactory, _mapper);
        }

        public IGroupGraphApiClient CreateGroupGraphApiClient(IAccessTokenManager accessTokenManager, INotifier notifier)
        {
            return new GroupGraphApiClient(accessTokenManager, notifier, _graphHttpProviderFactory, _graphServiceFactory, _mapper);
        }
    }
}

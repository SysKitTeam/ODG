using System;
using System.Threading.Tasks;
using Serilog;
using SysKit.ODG.App.Configuration;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.DTO.Generation.Results;
using SysKit.ODG.Base.Enums;
using SysKit.ODG.Base.Exceptions;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.XmlTemplate;
using SysKit.ODG.Generation;
using Unity;

namespace SysKit.ODG.App
{
    public interface IODGGenerator
    {
        /// <summary>
        /// Generates Microsoft 365 content based on specified template
        /// </summary>
        /// <param name="userCredentials"></param>
        /// <param name="clientId"></param>
        /// <param name="tenantDomain"></param>
        /// <param name="templateLocation"></param>
        /// <returns></returns>
        Task<GenerationResult> GenerateContent(SimpleUserCredentials userCredentials, string clientId, string tenantDomain, string templateLocation);

        /// <summary>
        /// Executes cleanup of groups and sites created by ODG
        /// </summary>
        /// <param name="userCredentials"></param>
        /// <param name="generationResult"></param>
        /// <param name="clientId"></param>
        /// <returns>True if there was 0 errors</returns>
        Task<bool> ExecuteCleanup(SimpleUserCredentials userCredentials, GenerationResult generationResult, string clientId);

        /// <summary>
        /// Executes cleanup of groups and sites created by ODG
        /// </summary>
        /// <param name="userCredentials"></param>
        /// <param name="cleanupFilePath"></param>
        /// <param name="clientId"></param>
        /// <returns>True if there was 0 errors</returns>
        Task<bool> ExecuteCleanup(SimpleUserCredentials userCredentials, string cleanupFilePath, string clientId);
        /// <summary>
        /// Save cleanup template
        /// </summary>
        /// <param name="generationResult"></param>
        /// <param name="templateLocation"></param>
        void SaveCleanupTemplate(GenerationResult generationResult, string templateLocation);
    }

    public class ODGGenerator : IODGGenerator
    {
        private static readonly UnityContainer _unityContainer;
        static ODGGenerator()
        {
            _unityContainer = UnityManager.CreateUnityContainer();
        }

        private readonly LogLevelEnum _logLevel;
        public ODGGenerator() : this(LogLevelEnum.Debug)
        {

        }

        public ODGGenerator(LogLevelEnum logLevel)
        {
            _logLevel = logLevel;
        }

        /// <inheritdoc />
        public Task<GenerationResult> GenerateContent(SimpleUserCredentials userCredentials, string clientId, string tenantDomain, string templateLocation)
        {
            var xmlService = new XmlSpecificationService();
            XmlODGTemplate template;

            try
            {
                template = xmlService.DeserializeSpecification<XmlODGTemplate>(templateLocation);
            }
            catch (Exception ex)
            {
                throw new XmlTemplateException(ex);
            }


            var accessTokenFactory = _unityContainer.Resolve<IAccessTokenManagerFactory>();
            var accessTokenManager = accessTokenFactory.CreateAccessTokenManager(userCredentials, clientId);

            var generationOptions = new GenerationOptions(accessTokenManager, userCredentials, tenantDomain, "1q32UmQx8Q", template);

            var generationService = _unityContainer.Resolve<IGenerationService>();
            generationService.AddGenerationTask("User Creation", _unityContainer.Resolve<IGenerationTask>("userTask"));
            generationService.AddGenerationTask("Group Creation", _unityContainer.Resolve<IGenerationTask>("groupTask"));
            generationService.AddGenerationTask("Site Creation", _unityContainer.Resolve<IGenerationTask>("siteTask"));
            return generationService.Start(generationOptions, createNotifier());
        }

        /// <inheritdoc />
        public Task<bool> ExecuteCleanup(SimpleUserCredentials userCredentials, GenerationResult generationResult, string clientId)
        {
            var generationCleanupService = _unityContainer.Resolve<IGenerationCleanupService>();

            var accessTokenFactory = _unityContainer.Resolve<IAccessTokenManagerFactory>();
            var accessTokenManager = accessTokenFactory.CreateAccessTokenManager(userCredentials, clientId);
            return generationCleanupService.ExecuteCleanup(userCredentials, generationResult, createNotifier(), accessTokenManager);
        }

        /// <inheritdoc />
        public Task<bool> ExecuteCleanup(SimpleUserCredentials userCredentials, string cleanupFilePath, string clientId)
        {
            var generationCleanupService = _unityContainer.Resolve<IGenerationCleanupService>();

            var accessTokenFactory = _unityContainer.Resolve<IAccessTokenManagerFactory>();
            var accessTokenManager = accessTokenFactory.CreateAccessTokenManager(userCredentials, clientId);
            return generationCleanupService.ExecuteCleanup(userCredentials, cleanupFilePath, createNotifier(), accessTokenManager);
        }

        /// <inheritdoc />
        public void SaveCleanupTemplate(GenerationResult generationResult, string templateLocation)
        {
            var generationCleanupService = _unityContainer.Resolve<IGenerationCleanupService>();
            generationCleanupService.SaveCleanupTemplate(generationResult, templateLocation);
        }


        private LoggNotifier createNotifier()
        {
            var logger = _unityContainer.Resolve<ILogger>();
            return new LoggNotifier(logger, new LoggOptions(_logLevel));
        }
    }
}

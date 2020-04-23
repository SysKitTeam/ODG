using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO.Generation.Results;
using SysKit.ODG.Base.Enums;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;
using SysKit.ODG.Base.Notifier;
using SysKit.ODG.Base.XmlCleanupTemplate;

namespace SysKit.ODG.Generation
{
    public class GenerationCleanupService : IGenerationCleanupService
    {
        private readonly XmlSpecificationService _specificationService;
        private readonly IUserDataGeneration _userDataGeneration;
        private readonly IGroupDataGeneration _groupDataGeneration;
        private readonly IGraphApiClientFactory _groupGraphApiClientFactory;
        private readonly ISharePointServiceFactory _sharePointServiceFactory;

        public GenerationCleanupService(XmlSpecificationService specificationService, 
            IUserDataGeneration userDataGeneration, 
            IGroupDataGeneration groupDataGeneration,
            IGraphApiClientFactory groupGraphApiClientFactory, 
            ISharePointServiceFactory sharePointServiceFactory)
        {
            _specificationService = specificationService;
            _userDataGeneration = userDataGeneration;
            _groupDataGeneration = groupDataGeneration;
            _groupGraphApiClientFactory = groupGraphApiClientFactory;
            _sharePointServiceFactory = sharePointServiceFactory;
        }

        /// <inheritdoc />
        public void SaveCleanupTemplate(GenerationResult result, string filePath)
        {
            var xmlCleanupTemplate = new XmlODGCleanupTemplate
            {
                TimeGenerated = DateTime.UtcNow
            };

            xmlCleanupTemplate.DirectoryElements = createCleanupElements(result).ToArray();
            _specificationService.SerializeSpecification(xmlCleanupTemplate, $"{filePath.Replace(".xml", "")}_{DateTime.Now:yyyy-dd-M--HH-mm}_cleanup.xml");
        }

        /// <inheritdoc />
        public Task<bool> ExecuteCleanup(SimpleUserCredentials credentials, GenerationResult result, INotifier notifier, IAccessTokenManager tokenManager)
        {
            var directoryElements = createCleanupElements(result);
            return executeCleanupInternal(credentials, directoryElements, notifier, tokenManager);
        }

        /// <inheritdoc />
        public Task<bool> ExecuteCleanup(SimpleUserCredentials credentials, string filePath, INotifier notifier, IAccessTokenManager tokenManager)
        {
            var template = _specificationService.DeserializeSpecification<XmlODGCleanupTemplate>(filePath);
            return executeCleanupInternal(credentials, template.DirectoryElements.ToList(), notifier, tokenManager);
        }

        public async Task<bool> executeCleanupInternal(SimpleUserCredentials credentials, List<XmlDirectoryElement> directoryElements, INotifier notifier, IAccessTokenManager tokenManager)
        {
            var hadErrors = false;

            foreach (var element in directoryElements)
            {
                switch (element.Type)
                {
                    case DirectoryElementTypeEnum.Team:
                    case DirectoryElementTypeEnum.UnifiedGroup:
                        hadErrors = hadErrors || (await deleteUnifiedGroup(credentials, element, notifier, tokenManager));
                        break;
                    case DirectoryElementTypeEnum.Site:
                        hadErrors = hadErrors || deleteSite(credentials, element, notifier);
                        break;
                    default:
                        break;
                }
            }

            return hadErrors;
        }

        protected async Task<bool> deleteUnifiedGroup(SimpleUserCredentials credentials, XmlDirectoryElement groupElement, INotifier notifier, IAccessTokenManager tokenManager)
        {
            var removedGroup = await _groupGraphApiClientFactory.CreateGroupGraphApiClient(tokenManager, notifier).DeleteUnifiedGroup(groupElement.Id);
            if (!removedGroup)
            {
                return false;
            }

            // no site to delete
            if (string.IsNullOrEmpty(groupElement.Url))
            {
                return true;
            }

            return _sharePointServiceFactory.Create(credentials, notifier).DeleteSiteCollection(groupElement.Url);
        }

        protected bool deleteSite(SimpleUserCredentials credentials, XmlDirectoryElement siteElement, INotifier notifier)
        {
            return _sharePointServiceFactory.Create(credentials, notifier).DeleteSiteCollection(siteElement.Url);
        }

        protected List<XmlDirectoryElement> createCleanupElements(GenerationResult result)
        {
            var directoryElements = new List<XmlDirectoryElement>();

            foreach (var taskResult in result.TaskResults)
            {
                if (taskResult is UserGenerationTaskResult userGenerationTask)
                {
                    directoryElements.AddRange(_userDataGeneration.CreateDirectoryElements(userGenerationTask.CreatedUsers));
                    continue;
                }

                if (taskResult is GroupGenerationTaskResult groupGenerationTask)
                {
                    directoryElements.AddRange(_groupDataGeneration.CreateDirectoryElements(groupGenerationTask.CreatedGroups));
                    continue;
                }

                if (taskResult is SiteGenerationTaskResult)
                {
                    continue;
                }

                throw new ArgumentException("Task result is not supported for creating a cleanup template");
            }

            return directoryElements;
        }
    }
}

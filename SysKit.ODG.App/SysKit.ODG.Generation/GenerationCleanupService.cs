using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.DTO.Generation.Results;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Notifier;
using SysKit.ODG.Base.XmlCleanupTemplate;

namespace SysKit.ODG.Generation
{
    public class GenerationCleanupService : IGenerationCleanupService
    {
        private readonly XmlSpecificationService _specificationService;
        private readonly IUserDataGeneration _userDataGeneration;
        private readonly IGroupDataGeneration _groupDataGeneration;

        public GenerationCleanupService(XmlSpecificationService specificationService, IUserDataGeneration userDataGeneration, IGroupDataGeneration groupDataGeneration)
        {
            _specificationService = specificationService;
            _userDataGeneration = userDataGeneration;
            _groupDataGeneration = groupDataGeneration;
        }

        public void SaveCleanupTemplate(GenerationResult result, string filePath)
        {
            var xmlCleanupTemplate = new XmlODGCleanupTemplate
            {
                TimeGenerated = DateTime.UtcNow
            };

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

                throw new ArgumentException("Task result is not supported for creating a cleanup template");
            }

            xmlCleanupTemplate.DirectoryElements = directoryElements.ToArray();
            _specificationService.SerializeSpecification(xmlCleanupTemplate, $"{filePath.Replace(".xml", "")}_cleanup.xml");
        }
    }
}

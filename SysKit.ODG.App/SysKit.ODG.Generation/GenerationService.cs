using SysKit.ODG.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.Interfaces.Office365Service;

namespace SysKit.ODG.Generation
{
    public class GenerationService: IGenerationService
    {
        private readonly List<IGenerationTask> _generationTasks = new List<IGenerationTask>();

        public GenerationService()
        {

        }

        public void AddGenerationTask(IGenerationTask task)
        {
            _generationTasks.Add(task);
        }

        public void Start(GenerationOptions generationOptions)
        {
            foreach (var task in _generationTasks)
            {
                task.Execute(generationOptions).GetAwaiter().GetResult();
            }

            //var testUsers = new List<UserEntry>();

            //for (var i = 21; i < 23; i++)
            //{
            //    testUsers.Add(new UserEntry
            //    {
            //        AccountEnabled = i > 8,
            //        DisplayName = $"Test User {i}",
            //        UserPrincipalName = $"testUser{i}@M365x314861.onmicrosoft.com",
            //        MailNickname = $"testUser{i}",
            //        Password = _configManager.DefaultUserPassword
            //    });
            //}

            //_userGraphApiClient.CreateTenantUsers(testUsers).GetAwaiter().GetResult();

            //Console.WriteLine(_configManager.ClientId);
            //Console.ReadLine();
        }
    }
}

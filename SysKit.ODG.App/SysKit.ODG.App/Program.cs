using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SysKit.ODG.App.Configuration;
using SysKit.ODG.Authentication;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
using SysKit.ODG.Base.Enums;
using SysKit.ODG.Base.Interfaces;
using SysKit.ODG.Base.Interfaces.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Base.XmlTemplate;
using SysKit.ODG.Base.XmlTemplate.Model;
using SysKit.ODG.Base.XmlTemplate.Model.Groups;
using SysKit.ODG.Generation;
using Unity;
using Unity.Lifetime;

namespace SysKit.ODG.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientId = "c24ff1fb-7b38-41d1-8b7a-681022bd3f37";
            var defaultPassword = "7q6keJrX5M";
            var tenantDomain = "M365B693698.onmicrosoft.com";
            var userCredentials = new SimpleUserCredentials("admin@M365B693698.onmicrosoft.com", "7q6keJrX5M");
            var testTemplate = new XmlODGTemplate
            {
                //RandomOptions = new XmlRandomOptions
                //{
                //    NumberOfUsers = 1000,
                //    NumberOfUnifiedGroups = 1000,
                //    NumberOfTeams = 100,
                //    MaxNumberOfOwnersPerGroup = 3,
                //    MaxNumberOfMembersPerGroup = 50
                //},
                Users = new []
                {
                    new XmlUser
                    {
                        Name = "dino.test"
                    },
                    new XmlUser
                    {
                        Name = "dino.test1"
                    },
                    new XmlUser
                    {
                        Name = "dino.test2"
                    },
                    new XmlUser
                    {
                        Name = "dino.test3"
                    },
                    new XmlUser
                    {
                        Name = "dino.test4"
                    }
                },
                Groups = new []
                {
                    new XmlUnifiedGroup
                    {
                        Name = "odg.new.group",
                        DisplayName = "ODG Group",
                        Members = new []
                        {
                            new XmlMember
                            {
                                Name = "dino.test"
                            }
                        }
                    },
                    new XmlTeam
                    {
                        Name = "odg.new.team2345",
                        DisplayName = "ODG Team with Provisioned channels 2",
                        Owners = new []
                        {
                            new XmlMember
                            {
                                Name = "dino.test2"
                            }
                        },
                        Members = new []
                        {
                            new XmlMember
                            {
                                Name = "dino.test1"
                            },
                            new XmlMember
                            {
                                Name = "dino.test2"
                            },
                            new XmlMember
                            {
                                Name = "adelev@M365B693698.onmicrosoft.com"
                            },
                            new XmlMember
                            {
                                Name = "admin@M365B693698.onmicrosoft.com"
                            }
                        },
                        Channels = new []
                        {
                            new XmlTeamChannel
                            {
                                DisplayName = "Custom public channel"
                            },
                            new XmlTeamChannel
                            {
                                DisplayName = "Custom public channel 2"
                            },
                            new XmlTeamChannel
                            {
                                DisplayName = "Custom Private channel",
                                IsPrivate = true,
                                Owners = new []
                                {
                                    new XmlMember
                                    {
                                        Name = "dino.test1"
                                    }
                                },
                                Members = new []
                                {
                                    new XmlMember
                                    {
                                        Name = "dino.test2"
                                    }
                                }
                            }
                        }
                    } 
                }
            };

            var xmlService = new XmlSpecificationService();
            //xmlService.SerializeSpecification(testTemplate, @"C:\Users\dino.kacavenda\test.xml");
            var template = xmlService.DeserializeSpecification(@"C:\Users\dino.kacavenda\test.xml");

            var unityContainer = UnityManager.CreateUnityContainer();
            var accessTokenFactory = unityContainer.Resolve<IAccessTokenManagerFactory>();
            var accessTokenManager = accessTokenFactory.CreateAccessTokenManager(userCredentials, clientId);
            var logger = unityContainer.Resolve<ILogger>();
            var notifier = new LoggNotifier(logger, new LoggOptions(LogLevelEnum.Debug));

            var generationOptions = new GenerationOptions(accessTokenManager, tenantDomain, defaultPassword, template);

            var generationService = unityContainer.Resolve<IGenerationService>();
            generationService.AddGenerationTask("User Creation", unityContainer.Resolve<IGenerationTask>("userTask"));
            generationService.AddGenerationTask("Group Creation", unityContainer.Resolve<IGenerationTask>("groupTask"));
            generationService.Start(generationOptions, notifier).GetAwaiter().GetResult();

            Console.WriteLine("Finished ;)");
            Console.ReadLine();
        }
    }
}

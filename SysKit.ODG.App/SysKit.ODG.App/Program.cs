using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.App.Configuration;
using SysKit.ODG.Authentication;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.DTO;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.DTO.Generation.Options;
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
            var defaultPassword = "hC7q95955D";
            var tenantDomain = "M365B117306.onmicrosoft.com";
            var userCredentials = new SimpleUserCredentials("admin@M365B117306.onmicrosoft.com", "hC7q95955D");
            var testTemplate = new XmlODGTemplate
            {
                RandomOptions = new XmlRandomOptions
                {
                    NumberOfUnifiedGroups = 5,
                    NumberOfTeams = 3,
                    MaxNumberOfOwnersPerGroup = 3,
                    MaxNumberOfMembersPerGroup = 10
                },
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
                    }
                },
                Groups = new []
                {
                    new XmlUnifiedGroup
                    {
                        Name = "Grupa sa ownerom",
                        DisplayName = "Grupica sa non mod admin ownerom",
                        Owners = new []
                        {
                            new XmlMember
                            {
                                Name = "dino.test"
                            }
                        },
                        IsPrivate = true
                    },
                    new XmlTeam
                    {
                        Name = "ODG Team",
                        DisplayName = "Team napravljen sa ODG",
                        Owners = new []
                        {
                            new XmlMember
                            {
                                Name = "dino.test"
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
                            }
                        },
                        Channels = new []
                        {
                            new XmlTeamChannel
                            {
                                DisplayName = "Standard channel"
                            },
                            new XmlTeamChannel
                            {
                                DisplayName = "Private channel",
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
                                        Name = "dino.test1"
                                    },
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
            xmlService.SerializeSpecification(testTemplate, @"C:\Users\dino.kacavenda\test.xml");
            var template = xmlService.DeserializeSpecification(@"C:\Users\dino.kacavenda\test.xml");

            var unityContainer = UnityManager.CreateUnityContainer();
            var accessTokenFactory = unityContainer.Resolve<IAccessTokenManagerFactory>();
            var accessTokenManager = accessTokenFactory.CreateAccessTokenManager(userCredentials);

            var generationOptions = new GenerationOptions(accessTokenManager, tenantDomain, defaultPassword, template);

            var generationService = unityContainer.Resolve<IGenerationService>();
            //generationService.AddGenerationTask("User Creation", unityContainer.Resolve<IGenerationTask>("userTask"));
            generationService.AddGenerationTask("Group Creation", unityContainer.Resolve<IGenerationTask>("groupTask"));
            generationService.Start(generationOptions).GetAwaiter().GetResult();

            Console.WriteLine("Finished ;)");
            Console.ReadLine();
        }
    }
}

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
            var userName = nonNullConsoleRead("Enter Global Admin username:");
            var tenantDomain = userName.Split('@')[1];

            Console.WriteLine("Enter Global Admin password:");
            var password = consolePassword();

            var clientId = nonNullConsoleRead("Enter client id:");

            var templateLocation = nonNullConsoleRead("ODG template location:");

            var userCredentials = new SimpleUserCredentials(userName, password);
            run(userCredentials, clientId, tenantDomain, password, templateLocation);
        }

        private static void run(SimpleUserCredentials userCredentials, string clientId, string tenantDomain, string defaultPassword, string templateLocation)
        {
            var xmlService = new XmlSpecificationService();
            //xmlService.SerializeSpecification(testTemplate, templateLocation);
            var template = xmlService.DeserializeSpecification(templateLocation);

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

        private static string nonNullConsoleRead(string message)
        {
            Console.WriteLine(message);
            var value = Console.ReadLine();

            if (string.IsNullOrEmpty(value))
            {
                return nonNullConsoleRead(message);
            }

            return value;
        }

        private static string consolePassword()
        {
            string pass = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);

            if (string.IsNullOrEmpty(pass))
            {
                return consolePassword();
            }

            return pass;
        }
    }
}

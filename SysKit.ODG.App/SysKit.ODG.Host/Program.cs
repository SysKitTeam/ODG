using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.App;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Enums;

namespace SysKit.ODG.Host
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
            var isCleanup = args?.Any() == true && args[0] == "clean";

            try
            {
                if (isCleanup)
                {
                    cleanup(userCredentials, clientId, templateLocation);
                }
                else
                {
                    generate(userCredentials, clientId, tenantDomain, templateLocation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Execution failed: {ex.Message}");
            }

            Console.WriteLine("Finished ;)");
            Console.ReadLine();
        }

        private static void generate(SimpleUserCredentials userCredentials, string clientId, string tenantDomain, string templateLocation)
        {
            var odgGenerator = new ODGGenerator(LogLevelEnum.Debug);
            var result = odgGenerator.GenerateContent(userCredentials, clientId, tenantDomain, templateLocation).GetAwaiter().GetResult();
            odgGenerator.SaveCleanupTemplate(result, templateLocation);
        }

        private static void cleanup(SimpleUserCredentials userCredentials, string clientId, string templateLocation)
        {
            var odgGenerator = new ODGGenerator(LogLevelEnum.Debug);
            var result = odgGenerator.ExecuteCleanup(userCredentials, templateLocation, clientId).GetAwaiter().GetResult();
            Console.WriteLine($"Clean had errors: {result}");
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

            Console.WriteLine();
            return pass;
        }
    }
}

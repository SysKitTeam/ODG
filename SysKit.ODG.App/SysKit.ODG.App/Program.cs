using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysKit.ODG.App.Configuration;

namespace SysKit.ODG.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = AppConfigManager.Create();
            Console.Read();

        }
    }
}

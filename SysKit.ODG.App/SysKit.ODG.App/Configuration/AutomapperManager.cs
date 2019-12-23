using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SysKit.ODG.Generation.Users;
using SysKit.ODG.Office365Service;

namespace SysKit.ODG.App.Configuration
{
    public class AutomapperManager
    {
        public static IMapper ConfigureMapper()
        {
            return new MapperConfiguration(config =>
            {
                config.AddProfile<Office365MapProfile>();
                config.AddProfile<UserMapProfile>();
            }).CreateMapper();
        }
    }
}

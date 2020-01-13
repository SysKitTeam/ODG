using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Graph;
using SysKit.ODG.Base.DTO.Generation;

namespace SysKit.ODG.Office365Service
{
    public class Office365MapProfile: Profile
    {
        public Office365MapProfile()
        {
            #region Users

            CreateMap<UserEntry, User>();

            #endregion Users
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.XMLSpecification.Model;

namespace SysKit.ODG.Generation.Users
{
    public class UserMapProfile: Profile
    {
        public UserMapProfile()
        {
            CreateMap<XmlUser, UserEntry>();
        }
    }
}

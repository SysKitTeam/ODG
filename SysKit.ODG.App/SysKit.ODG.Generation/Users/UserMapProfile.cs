﻿using AutoMapper;
using Microsoft.Graph;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Enums;
using SysKit.ODG.Base.XmlCleanupTemplate;
using SysKit.ODG.Base.XmlTemplate.Model;

namespace SysKit.ODG.Generation.Users
{
    public class UserMapProfile: Profile
    {
        public UserMapProfile()
        {
            CreateMap<User, UserEntry>();
            // will skip over all null values. Used to populate default values
            CreateMap<UserEntry, UserEntry>().ForAllMembers(o => o.Condition((source, destination, member) => member != null));

            CreateMap<UserEntry, XmlDirectoryElement>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => DirectoryElementTypeEnum.User));
        }
    }
}

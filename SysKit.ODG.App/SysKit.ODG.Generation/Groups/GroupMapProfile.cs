using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Enums;
using SysKit.ODG.Base.XmlCleanupTemplate;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupMapProfile: Profile
    {
        public GroupMapProfile()
        {
            CreateMap<GroupEntry, XmlDirectoryElement>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src is UnifiedGroupEntry ? DirectoryElementTypeEnum.UnifiedGroup : DirectoryElementTypeEnum.Group));
        }
    }
}

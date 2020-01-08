using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupMapProfile: Profile
    {
        public GroupMapProfile()
        {
            //CreateMap<XmlUser, UserEntry>();
            //// will skip over all null values. Used to populate default values
            //CreateMap<UserEntry, UserEntry>().ForAllMembers(o => o.Condition((source, destination, member) => member != null));
        }
    }
}

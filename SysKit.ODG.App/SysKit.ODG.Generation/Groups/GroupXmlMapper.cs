using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;

namespace SysKit.ODG.Generation.Groups
{
    public class GroupXmlMapper
    {
        private readonly IMapper _mapper;

        public GroupXmlMapper(IMapper mapper)
        {
            _mapper = mapper;
        }
    }
}

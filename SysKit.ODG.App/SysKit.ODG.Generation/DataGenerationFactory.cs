using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.Generation.Users;
using SysKit.ODG.XMLSpecification;
using SysKit.ODG.XMLSpecification.Model;

namespace SysKit.ODG.Generation
{
    public class DataGenerationFactory : IDataGenerationFactory
    {
        private readonly IMapper _mapper;
        public DataGenerationFactory(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IEnumerable<UserEntry> GetUserData(IGenerationOptions options)
        {
            if (options is XmlGenerationOptions xmlOptions)
            {
                return new UserDataGeneration(_mapper, xmlOptions.XmlTemplate).CreateUsers();
            }

            throw new ArgumentException("There is no user data generator for specified IGenerationOptions type");
        }
    }
}

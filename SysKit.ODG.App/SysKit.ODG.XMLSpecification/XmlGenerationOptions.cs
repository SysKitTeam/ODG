using System;
using System.Collections.Generic;
using System.Text;
using SysKit.ODG.Base.Authentication;
using SysKit.ODG.Base.Interfaces.Generation;
using SysKit.ODG.XMLSpecification.Model;

namespace SysKit.ODG.XMLSpecification
{
    public class XmlGenerationOptions: IGenerationOptions
    {
        public SimpleUserCredentials UserCredentials { get; }
        public string TenantDomain { get; private set; }
        public string DefaultPassword { get; set; }
        public XmlODGSpecification XmlTemplate { get; }

        public XmlGenerationOptions(SimpleUserCredentials userCredentials, XmlODGSpecification template)
        {
            UserCredentials = userCredentials;
            XmlTemplate = template;
            TenantDomain = userCredentials.Username.Split('@')[1];
        }
    }
}

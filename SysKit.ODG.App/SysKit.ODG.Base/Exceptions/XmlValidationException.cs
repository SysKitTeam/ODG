using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Exceptions
{
    public class XmlValidationException: Exception
    {
        public XmlValidationException(string message) : base($"There was an error with xml template: {message}")
        {

        }
    }
}

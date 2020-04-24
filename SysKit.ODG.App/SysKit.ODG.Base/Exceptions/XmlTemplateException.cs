using System;

namespace SysKit.ODG.Base.Exceptions
{
    public class XmlTemplateException : Exception
    {
        public XmlTemplateException(Exception innerException) : base("There is an error with your ODG Template", innerException)
        {

        }
    }
}

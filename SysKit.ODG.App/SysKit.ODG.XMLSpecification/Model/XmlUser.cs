using SysKit.ODG.Base.Interfaces.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace SysKit.ODG.XMLSpecification.Model
{
    public class XmlUser : IUser
    {
        [XmlAttribute]
        public bool? AccountEnabled { get; set; }
        [XmlAttribute]
        public string DisplayName { get; set; }
        [XmlAttribute]
        public string MailNickname { get; set; }
        [XmlAttribute]
        public string UserPrincipalName { get; set; }
        [XmlAttribute]
        public string Password { get; set; }
        [XmlAttribute]
        public bool? SetUserPhoto { get; set; }
    }
}

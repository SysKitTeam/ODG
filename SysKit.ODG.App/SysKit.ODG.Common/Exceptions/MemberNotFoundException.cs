using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Exceptions
{
    public class MemberNotFoundException: Exception
    {
        public string Name { get; }
        public MemberNotFoundException(string name) : base($"User with {name} was not found")
        {
            Name = name;
        }
    }
}

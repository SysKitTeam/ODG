using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.DTO.Generation
{
    public class MemberEntry: IEquatable<MemberEntry>
    {
        public string Name { get; set; }

        public MemberEntry(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Checks if NAME is full LoginName (with tenant)
        /// </summary>
        public bool IsFQDN => Name.Contains("@");

        public bool Equals(MemberEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MemberEntry) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}

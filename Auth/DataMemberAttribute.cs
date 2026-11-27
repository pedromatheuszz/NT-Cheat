using System;

namespace KeyAuth
{
    internal class DataMemberAttribute : Attribute
    {
        public bool IsRequired { get; set; }
        public bool EmitDefaultValue { get; set; }
    }
}
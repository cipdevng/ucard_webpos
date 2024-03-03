using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Model.Attributes {
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class IgnoreProperty : Attribute {
        private readonly bool _ignore = true;
    }
}

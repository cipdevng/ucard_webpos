using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Model.Attributes {
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class NumberAttribute : Attribute {
        public int length { get; private set; }
        public string defualtValue { get; protected set; }
        public bool _autoIncrement;
        public NumberAttribute(int length = 100, string? _defaultValue = null, bool autoIncrement = false) {
            this.length = length;
            this.defualtValue = _defaultValue;
            this._autoIncrement = autoIncrement;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Model.Attributes {
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class DBIndex : Attribute {
        public IndexAttributes indexAttribute { get; private set; }

        public DBIndex(IndexAttributes attribute = IndexAttributes.NONE) {
            this.indexAttribute = attribute;
        }
    }

    public enum IndexAttributes {
        UNIQUE = 1,
        FULL_TEXT_INDEX = 2,
        NONE = 0,
        UNIQUE_AND_PRIMARY = 3
    }
}
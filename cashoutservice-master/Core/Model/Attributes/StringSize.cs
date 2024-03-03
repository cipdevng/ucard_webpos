using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Model.Attributes {
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class StringAttribute : Attribute{
        public string SQLEquiv { get; private set; }
        public int length { get; private set; }
        public string defualtValue { get; protected set; }
        public string allowNull { get; protected set; }
        public StringAttribute(int length = 100, string? defaultValue = null, bool allowNull = true) {
            this.length = length;
            if (length < 1000) {
                SQLEquiv = "VARCHAR("+length+")";
            } else {
                SQLEquiv = "TEXT";
            }
            if (defualtValue is null) {
                if(SQLEquiv != "TEXT") {
                    this.defualtValue = "DEFAULT NULL";
                }
            } else {
                this.defualtValue = string.Concat("DEFAULT '", defualtValue, "'");
            }
            if (!allowNull) {
                this.allowNull = "NOT NULL";
                if (defaultValue is null)
                    this.defualtValue = string.Empty;
            }
        }
    }
}

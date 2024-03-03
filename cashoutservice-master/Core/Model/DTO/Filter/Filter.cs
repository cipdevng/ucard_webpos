using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Model.DTO.Filter {
    public class Filter {
        protected Dictionary<string, object> reference = new Dictionary<string, object>();
        protected string GetCallerName([CallerMemberName] string? name = null) {
            return name;
        }
        public void unsetField(string field) {
            try {
                reference.Remove(field);
            } catch { }            
        }
        public FindOperation findOp {
            get {
                string thisName = GetCallerName();
                if (!fieldIsSet(thisName))
                    return FindOperation.AND;
                return (FindOperation)getField(thisName);
            }
            set {
                string thisName = GetCallerName();
                if (fieldIsSet(thisName)) {
                    reference[thisName] = value;
                } else {
                    reference.Add(thisName, value);
                }
            }
        }
        protected object getField(string field) {
            try {
                return reference[field];
            } catch {
                throw new Exception("Field was not set");
            }
        }

        public bool fieldIsSet(string field) {
            object value;
            if (reference.TryGetValue(field, out value)) {
                return true;
            }
            return false;
        }

        protected void setValue<T>(T value, string caller) {
            if (fieldIsSet(caller)) {
                reference[caller] = value;
            } else {
                reference.Add(caller, value);
            }
        }
        protected T getValue<T>(string caller) {
            string thisName = GetCallerName();
            return (T)getField(caller);
        }

        public string orderClause {
            get {
                return getValue<string>("orderClause");
            }
            set {
                setValue<string>(value, "orderClause");
            }
        }
    }
    public enum FindOperation {
        AND, OR
    }
}

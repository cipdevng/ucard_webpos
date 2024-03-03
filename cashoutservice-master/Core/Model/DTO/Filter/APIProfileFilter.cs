using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Filter {
    public class APIProfileFilter : Filter {
        public string publicKey {
            get {
                string thisName = GetCallerName();
                return getValue<string>(thisName);
            }
            set {
                string thisName = GetCallerName();
                setValue<string>(value, thisName);
            }
        }
        public string secretKey {
            get {
                string thisName = GetCallerName();
                return getValue<string>(thisName);
            }
            set {
                string thisName = GetCallerName();
                setValue<string>(value, thisName);
            }
        }
    }
}

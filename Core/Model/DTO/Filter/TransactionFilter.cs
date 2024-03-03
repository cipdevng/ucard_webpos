using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Filter {
    public class TransactionFilter : Filter {
        public int id {
            get {
                string thisName = GetCallerName();
                return getValue<int>(thisName);
            }
            set {
                string thisName = GetCallerName();
                setValue<int>(value, thisName);
            }
        }

        public string transID {
            get {
                string thisName = GetCallerName();
                return getValue<string>(thisName);
            }
            set {
                string thisName = GetCallerName();
                setValue<string>(value, thisName);
            }
        }
        public string apiProfile {
            get {
                string thisName = GetCallerName();
                return getValue<string>(thisName);
            }
            set {
                string thisName = GetCallerName();
                setValue<string>(value, thisName);
            }
        }
        public string deviceID {
            get {
                string thisName = GetCallerName();
                return getValue<string>(thisName);
            }
            set {
                string thisName = GetCallerName();
                setValue<string>(value, thisName);
            }
        }
        public Channels channel {
            get {
                string thisName = GetCallerName();
                return getValue<Channels>(thisName);
            }
            set {
                string thisName = GetCallerName();
                setValue<Channels>(value, thisName);
            }
        }

        public (long fromDate, long toDate) transDate {
            get {
                string thisName = GetCallerName();
                return getValue<(long, long)>(thisName);
            }
            set {
                string thisName = GetCallerName();
                setValue<(long, long)>(value, thisName);
            }
        }
    }
}

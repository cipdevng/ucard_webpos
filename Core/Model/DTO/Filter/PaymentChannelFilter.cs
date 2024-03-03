using Core.Model.Entities;
using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Filter {
    public class PaymentChannelFilter : Filter {
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

        public ChannelStatus status {
            get {
                string thisName = GetCallerName();
                return getValue<ChannelStatus>(thisName);
            }
            set {
                string thisName = GetCallerName();
                setValue<ChannelStatus>(value, thisName);
            }
        }

        public double amount {
            get {
                string thisName = GetCallerName();
                return getValue<double>(thisName);
            }
            set {
                string thisName = GetCallerName();
                setValue<double>(value, thisName);
            }
        }
    }
}

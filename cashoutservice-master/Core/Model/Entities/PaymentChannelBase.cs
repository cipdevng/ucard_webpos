using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Entities {
    public class PaymentChannelBase : BaseEntity {
        public Channels channel { get; protected set; }
        public string channelName {
            get {
                return channel.ToString();
            }
        }
        public ChannelStatus status { get; protected set; }
        public string statusDescription {
            get {
                return status.ToString();
            }
        }
        public int priority { get; protected set; }
        public int transactionCount { get; protected set; }
        public int transactionCountFailure { get; protected set; }
        public long dateCreated { get; protected set; }
        public double failureRate {
            get {
                return transactionCount < 1? 0: (transactionCountFailure / transactionCount) * 100;
            }
        }
    }
}

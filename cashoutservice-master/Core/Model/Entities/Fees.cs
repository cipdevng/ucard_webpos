using Core.Application.Exceptions;
using Core.Model.DTO.Request;
using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Entities {
    public class Fees : BaseEntity {
        public double upperTransactionBound { get; protected set; }
        public double lowerTransactionBound { get; protected set; }
        public double chargeValue { get; protected set; }
        public int chargeIsFlat { get; protected set; }
        public double cap { get; protected set; }
        public Channels channel { get; protected set; }
        public Fees() { }
        public Fees(FeesDTO fees, PaymentChannelBase channel) {
            if (fees.lowerTransactionBound > fees.upperTransactionBound)
                throw new InputError("Upper transaction band cannot be lower than Lower's");
            if(fees.chargeIsFlat == 1 && (fees.chargeValue > 100 || fees.chargeValue < 0 ))
                throw new InputError("Invalid fee");
            this.upperTransactionBound = fees.upperTransactionBound;
            this.lowerTransactionBound = fees.lowerTransactionBound;
            this.chargeValue = fees.chargeValue;
            this.chargeIsFlat = fees.chargeIsFlat;
            this.cap = fees.cap;
            this.channel = channel.channel;
        }
    }
}

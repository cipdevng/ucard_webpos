using Core.Application.Exceptions;
using Core.Model.DTO.Request;
using Core.Model.Enums;
using Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Entities {
    public class PaymentChannels : PaymentChannelBase {        
        public double upperTransactionBound { get; private set; }
        public double lowerTransactionBound { get; private set; }
        public double chargeValue { get; private set; }
        public int chargeIsFlat { get; private set; }
        public double cap { get; private set; }
        public PaymentChannels() {

        }
        public PaymentChannels(PaymentChannelDTO dto) {
            //cannotBeNullOrEmpty(dto.channelName);
            if (dto.channel == null || dto.status == null)
                throw new InputError("Invalid Channel and status");
            this.channel = (Channels)dto.channel;;
            this.status = (ChannelStatus)dto.status;
            this.priority = dto.priority;
            this.transactionCount = 0;
            this.transactionCountFailure = 0;
            this.dateCreated = Utilities.getTodayDate().unixTimestamp;
        }
        public double getFee(double amount) {
            if (this.chargeIsFlat == 1)
                return this.chargeValue;
            var perc = ((chargeValue / 100) * amount);
            if (perc > cap)
                return cap;
            return perc;
        }
    }
}

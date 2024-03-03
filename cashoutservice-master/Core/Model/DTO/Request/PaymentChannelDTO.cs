using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Request {
    public class PaymentChannelDTO {
        public int id { get; set; }
        public Channels? channel { get; set; } = null;
        public string channelName { get; set; }
        public ChannelStatus? status { get; set; } = null;
        public string statusDescription { get; set; }
        public int priority { get; set; }
        public int transactionCount { get; set; }
        public int transactionCountFailure { get; set; }
        public long dateCreated { get; set; }
        public List<FeesDTO> fees { get; set; }
    }
}

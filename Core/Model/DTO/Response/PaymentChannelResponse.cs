using Core.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Response {
    public class PaymentChannelResponse : PaymentChannelBase {
        public List<Fees> fees { get; set; }
    }
}

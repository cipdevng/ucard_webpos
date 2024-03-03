using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Request {
    public class FeesDTO {
        public double upperTransactionBound { get; set; }
        public double lowerTransactionBound { get; set; }
        public double chargeValue { get; set; }
        public int chargeIsFlat { get; set; }
        public double cap { get; set; }
        public Channels channel { get; set; }
    }
}

using Iso8583CS.Attributes;
using Iso8583CS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UP_Sample.ISOTemplates.UP;

namespace Infrastructure.DTO.ISOTemplates.UP {
    public class PayAllCardsRequestVisa : PayAllCardsRequest {
        [IsoField(position: IsoFields.Field055, maxLen: 236, lengthType: LengthType.FIXED, contentType: ContentType.ANS)]
        public override string iccSystemRelated { get; set; }

        [IsoField(position: IsoFields.Field035, maxLen: 32, lengthType: LengthType.LLVAR, contentType: ContentType.Z)]
        public override string track2Data { get; set; }
    }
}

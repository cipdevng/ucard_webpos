using Iso8583CS.Attributes;
using Iso8583CS.Common;
using Iso8583CS.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO.ISOTemplates.UP {
    public class ParameterDownloadRequest : BaseMessage {
        [IsoField(position: IsoFields.Field003, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string processingCode { get; set; }

        [IsoField(position: IsoFields.Field007, maxLen: 10, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string transmissionDateAndTime { get; set; }

        [IsoField(position: IsoFields.Field011, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string STAN { get; set; }

        [IsoField(position: IsoFields.Field012, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string localTime { get; set; }

        [IsoField(position: IsoFields.Field013, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string localDate { get; set; }

        [IsoField(position: IsoFields.Field041, maxLen: 8, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string terminalID { get; set; }

        [IsoField(position: IsoFields.Field062, maxLen: 8, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string privateField { get; set; }

        [IsoField(position: IsoFields.Field064, maxLen: 64, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string primaryMessageHashValue { get; set; }
    }

    public class ParameterDownloadRequestArca : BaseMessage {
        [IsoField(position: IsoFields.Field003, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string processingCode { get; set; }

        [IsoField(position: IsoFields.Field007, maxLen: 10, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string transmissionDateAndTime { get; set; }

        [IsoField(position: IsoFields.Field011, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string STAN { get; set; }

        [IsoField(position: IsoFields.Field012, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string localTime { get; set; }

        [IsoField(position: IsoFields.Field013, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string localDate { get; set; }

        [IsoField(position: IsoFields.Field041, maxLen: 8, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string terminalID { get; set; }

        [IsoField(position: IsoFields.Field062, maxLen: 999, lengthType: LengthType.LLLVAR, contentType: ContentType.AN)]
        public string privateField { get; set; }

        [IsoField(position: IsoFields.Field064, maxLen: 64, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string primaryMessageHashValue { get; set; }
    }
}

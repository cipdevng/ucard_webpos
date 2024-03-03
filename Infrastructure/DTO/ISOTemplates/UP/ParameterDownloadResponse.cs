using Core.Model.DTO.Configuration;
using Core.Model.Enums;
using Infrastructure.DTO.Enums;
using Iso8583CS.Attributes;
using Iso8583CS.Common;
using Iso8583CS.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO.ISOTemplates.UP {
    public class ParameterDownloadResponse : BaseMessage {
        [IsoField(position: IsoFields.Field007, maxLen: 10, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string transmissionDateAndTime { get; set; }

        [IsoField(position: IsoFields.Field003, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string processingCode { get; set; }

        [IsoField(position: IsoFields.Field011, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string STAN { get; set; }

        [IsoField(position: IsoFields.Field012, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string localTime { get; set; }

        [IsoField(position: IsoFields.Field013, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string localDate { get; set; }

        [IsoField(position: IsoFields.Field039, maxLen: 2, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string responseCode { get; set; }

        [IsoField(position: IsoFields.Field041, maxLen: 8, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string terminalID { get; set; }

        [IsoField(position: IsoFields.Field062, maxLen: 123, lengthType: LengthType.LLLVAR, contentType: ContentType.S)]
        public string privateField { get; set; }

        [IsoField(position: IsoFields.Field064, maxLen: 64, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string primaryMessageHashValue { get; set; }

        public Parameters getParameters() {
            Parameters param = new Parameters();
            for (int i = 0; i < this.privateField?.Length;){
                if (this.privateField.Length - i < 6)
                    break;
                var field = int.Parse(this.privateField.Substring(i, 2));
                var length = int.Parse(this.privateField.Substring(i+2, 3));
                var data = this.privateField.Substring(i + 5, length);
                param.assign((Tags)field, data);
                i = i + 5 + length;
            }
            return param;
        }
    }
}

using System;
using Iso8583CS.Attributes;
using Iso8583CS.Messages;
using Iso8583CS.Common;

namespace UP_Sample.ISOTemplates.UP {
    public class PayAllCardsReponse : BaseMessage {
        [IsoField(position: IsoFields.Field002, maxLen: 18, lengthType: LengthType.FIXED, contentType: ContentType.AN, DataType.ASCII)]
        public string PAN { get; set; }

        [IsoField(position: IsoFields.Field003, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string processingCode { get; set; }

        [IsoField(position: IsoFields.Field004, maxLen: 12, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string amountTransaction { get; set; }

        [IsoField(position: IsoFields.Field007, maxLen: 10, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string transmissionDateAndTime { get; set; }

        [IsoField(position: IsoFields.Field011, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string STAN { get; set; }

        [IsoField(position: IsoFields.Field012, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string localTime { get; set; }

        [IsoField(position: IsoFields.Field013, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string localDate { get; set; }

        [IsoField(position: IsoFields.Field014, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string expirationDate { get; set; }

        [IsoField(position: IsoFields.Field018, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string merchantsType { get; set; }

        [IsoField(position: IsoFields.Field022, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string posEntryMode { get; set; }

        [IsoField(position: IsoFields.Field023, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string CSN { get; set; }

        [IsoField(position: IsoFields.Field025, maxLen: 2, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string posCondition { get; set; }

        [IsoField(position: IsoFields.Field026, maxLen: 2, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string posPinCaptureCode { get; set; }

        [IsoField(position: IsoFields.Field028, maxLen: 9, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string amountTransactionFee2 { get; set; }

        [IsoField(position: IsoFields.Field032, maxLen: 8, lengthType: LengthType.FIXED, contentType: ContentType.AN, DataType.ASCII)]
        public string acquiringInstitutionIdCode { get; set; }

        [IsoField(position: IsoFields.Field033, maxLen: 8, lengthType: LengthType.FIXED, contentType: ContentType.AN, DataType.ASCII)]
        public string forwardingInstitutionID { get; set; }

        [IsoField(position: IsoFields.Field035, maxLen: 36, lengthType: LengthType.LLVAR, contentType: ContentType.AN, DataType.ASCII)]
        public string track2Data { get; set; }

        [IsoField(position: IsoFields.Field037, maxLen: 12, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string retrievalReferenceNumber { get; set; }

        [IsoField(position: IsoFields.Field038, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string authorizationIdResponse { get; set; }

        [IsoField(position: IsoFields.Field039, maxLen: 2, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string responseCode { get; set; }

        [IsoField(position: IsoFields.Field040, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string serviceRestrictionCode { get; set; }

        [IsoField(position: IsoFields.Field041, maxLen: 8, lengthType: LengthType.FIXED, contentType: ContentType.AN, DataType.ASCII)]
        public string cardAcceptorTerminalId { get; set; }

        [IsoField(position: IsoFields.Field042, maxLen: 15, lengthType: LengthType.FIXED, contentType: ContentType.AN, DataType.ASCII)]
        public string cardAcceptorIdCode { get; set; }

        [IsoField(position: IsoFields.Field043, maxLen: 40, lengthType: LengthType.FIXED, contentType: ContentType.AN, DataType.ASCII)]
        public string cardAcceptorNameLocation { get; set; }

        [IsoField(position: IsoFields.Field049, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N, DataType.ASCII)]
        public string currencyCode { get; set; }

        [IsoField(position: IsoFields.Field054, maxLen: 43, lengthType: LengthType.FIXED, contentType: ContentType.AN, DataType.ASCII)]
        public string amount2 { get; set; }

        [IsoField(position: IsoFields.Field123, maxLen: 18, lengthType: LengthType.FIXED, contentType: ContentType.AN, DataType.ASCII)]
        public string posDataCode { get; set; }

        [IsoField(position: IsoFields.Field128, maxLen: 32, lengthType: LengthType.FIXED, contentType: ContentType.AN, DataType.ASCII)]
        public string secondaryMessageHashValue { get; set; }
    }
}


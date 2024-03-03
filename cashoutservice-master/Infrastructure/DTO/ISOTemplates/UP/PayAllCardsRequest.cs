using System;
using Iso8583CS.Attributes;
using Iso8583CS.Messages;
using Iso8583CS.Common;

namespace UP_Sample.ISOTemplates.UP {
    public class PayAllCardsRequest : BaseMessage {

        [IsoField(position: IsoFields.Field002, maxLen: 19, lengthType: LengthType.LLVAR, contentType: ContentType.N)]
        public virtual string PAN { get; set; }

        [IsoField(position: IsoFields.Field003, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string processingCode { get; set; }

        [IsoField(position: IsoFields.Field004, maxLen: 12, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string amountTransaction { get; set; }

        [IsoField(position: IsoFields.Field007, maxLen: 10, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string transmissionDateAndTime { get; set; }

        [IsoField(position: IsoFields.Field011, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string STAN { get; set; }

        [IsoField(position: IsoFields.Field012, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string localTime { get; set; }

        [IsoField(position: IsoFields.Field013, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string localDate { get; set; }

        [IsoField(position: IsoFields.Field014, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string expirationDate { get; set; }

        [IsoField(position: IsoFields.Field018, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string merchantsType { get; set; }

        [IsoField(position: IsoFields.Field022, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string posEntryMode { get; set; }

        [IsoField(position: IsoFields.Field023, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string CSN { get; set; }

        [IsoField(position: IsoFields.Field025, maxLen: 2, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string posCondition { get; set; }

        [IsoField(position: IsoFields.Field026, maxLen: 2, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string posPinCaptureCode { get; set; }

        [IsoField(position: IsoFields.Field028, maxLen: 9, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string amountTransactionFee2 { get; set; }

        [IsoField(position: IsoFields.Field032, maxLen: 11, lengthType: LengthType.LLVAR, contentType: ContentType.AN)]
        public string acquiringInstitutionIdCode { get; set; }

        /*[IsoField(position: IsoFields.Field033, maxLen: 11, lengthType: LengthType.LLVAR, contentType: ContentType.N)]
        public string forwardingInstitutionID { get; set; }*/

        [IsoField(position: IsoFields.Field035, maxLen: 34, lengthType: LengthType.LLVAR, contentType: ContentType.Z)]
        public virtual string track2Data { get; set; }

        [IsoField(position: IsoFields.Field037, maxLen: 12, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string retrievalReferenceNumber { get; set; }

        [IsoField(position: IsoFields.Field040, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string serviceRestrictionCode { get; set; }

        [IsoField(position: IsoFields.Field041, maxLen: 8, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string cardAcceptorTerminalId { get; set; }

        [IsoField(position: IsoFields.Field042, maxLen: 15, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string cardAcceptorIdCode { get; set; }

        [IsoField(position: IsoFields.Field043, maxLen: 40, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string cardAcceptorNameLocation { get; set; }

        [IsoField(position: IsoFields.Field049, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N)]
        public string currencyCode { get; set; }

        [IsoField(position: IsoFields.Field052, maxLen: 16, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string pinBlock { get; set; }

        [IsoField(position: IsoFields.Field055, maxLen: 236, lengthType: LengthType.FIXED, contentType: ContentType.ANS)]
        public virtual string iccSystemRelated { get; set; }

        [IsoField(position: IsoFields.Field123, maxLen: 15, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string posDataCode { get; set; }

        /*[IsoField(position: IsoFields.Field124, maxLen: 255, lengthType: LengthType.LVAR, contentType: ContentType.B)]
        public string nfcData { get; set; }*/

        [IsoField(position: IsoFields.Field128, maxLen: 64, lengthType: LengthType.FIXED, contentType: ContentType.AN)]
        public string secondaryHash { get; set; }
    }
}


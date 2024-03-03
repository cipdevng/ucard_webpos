using Iso8583CS.Attributes;
using Iso8583CS.Messages;
using Iso8583CS.Common;
public class RefundResponse: BaseMessage {

    [IsoField(position: IsoFields.Field002, maxLen: 19, lengthType: LengthType.LVAR, contentType: ContentType.N)]
    public string primaryAccountNumber { get; set; }

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

    [IsoField(position: IsoFields.Field013, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string localDate { get; set; }

    [IsoField(position: IsoFields.Field014, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string expirationDate { get; set; }

    [IsoField(position: IsoFields.Field018, maxLen: 4, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string merchantsType { get; set; }

    [IsoField(position: IsoFields.Field022, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string posEntryMode { get; set; }

    [IsoField(position: IsoFields.Field023, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string cardSequenceNumber { get; set; }

    [IsoField(position: IsoFields.Field025, maxLen: 2, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string posPinCaptureCode { get; set; }

    [IsoField(position: IsoFields.Field026, maxLen: 12, lengthType: LengthType.LVAR, contentType: ContentType.N)]
    public string amountTransactionFee { get; set; }

    // [IsoField(position: IsoFields.Field028, maxLen: 9, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    // public string amountTransactionFee { get; set; }

    [IsoField(position: IsoFields.Field032, maxLen: 11, lengthType: LengthType.LVAR, contentType: ContentType.N)]
    public string acquiringInstitutionIdCode { get; set; }

    [IsoField(position: IsoFields.Field033, maxLen: 11, lengthType: LengthType.LVAR, contentType: ContentType.N)]
    public string forwardingInstitutionID { get; set; }

    [IsoField(position: IsoFields.Field035, maxLen: 37, lengthType: LengthType.LVAR, contentType: ContentType.Z)]
    public string track2Data { get; set; }

    [IsoField(position: IsoFields.Field037, maxLen: 12, lengthType: LengthType.LVAR, contentType: ContentType.N)]
    public string retrievalReferenceNumber { get; set; }

    [IsoField(position: IsoFields.Field038, maxLen: 6, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string authorizationIdResponse { get; set; }

    [IsoField(position: IsoFields.Field039, maxLen: 2, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string responseCode { get; set; }

    [IsoField(position: IsoFields.Field040, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string serviceRestrictionCode { get; set; }

    [IsoField(position: IsoFields.Field041, maxLen: 8, lengthType: LengthType.LVAR, contentType: ContentType.AN)]
    public string cardAcceptorTerminalId { get; set; }

    [IsoField(position: IsoFields.Field042, maxLen: 15, lengthType: LengthType.LVAR, contentType: ContentType.AN)]
    public string cardAcceptorIdCode { get; set; }

    [IsoField(position: IsoFields.Field043, maxLen: 40, lengthType: LengthType.LVAR, contentType: ContentType.AN)]
    public string cardAcceptorNameLocation { get; set; }

    [IsoField(position: IsoFields.Field049, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string currencyCodeTransaction { get; set; }

    [IsoField(position: IsoFields.Field055, maxLen: 999, lengthType: LengthType.LVAR, contentType: ContentType.B)]
    public string iccSystemRelated { get; set; }

    [IsoField(position: IsoFields.Field123, maxLen: 3, lengthType: LengthType.FIXED, contentType: ContentType.N)]
    public string posDataCode { get; set; }

    [IsoField(position: IsoFields.Field128, maxLen: 32, lengthType: LengthType.LVAR, contentType: ContentType.B)]
    public string secondaryMessageHashValue { get; set; }
}
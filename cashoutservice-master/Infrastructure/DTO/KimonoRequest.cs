using Core.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Infrastructure.Model {
    public class KimonoRequest {
		public class Authentication {
			[XmlRoot(ElementName = "terminalInformation")]
			public class TerminalInformation {
				[XmlElement(ElementName = "merchantId")]
				public string MerchantId { get; set; }
				[XmlElement(ElementName = "terminalId")]
				public string TerminalId { get; set; }
			}

			[XmlRoot(ElementName = "tokenPassportRequest")]
			public class TokenPassportRequest {
				[XmlElement(ElementName = "terminalInformation")]
				public TerminalInformation TerminalInformation { get; set; }
			}

			[XmlRoot(ElementName = "tokenPassportResponse")]
			public class TokenPassportResponse {
				[XmlElement(ElementName = "responseCode")]
				public string ResponseCode { get; set; }
				[XmlElement(ElementName = "responseMessage")]
				public string ResponseMessage { get; set; }
				[XmlElement(ElementName = "token")]
				public string Token { get; set; }
				public long dateCreated { get; set; }
				public string terminalID { get; set; }
				public bool sessionIsValid() {
					return this.dateCreated > Utilities.getTodayDate().unixTimestamp - 5 * 3600;
                }
			}
		}
		
		public class Purchase {
			[XmlRoot(ElementName = "terminalInformation")]
			public class TerminalInformation {
				[XmlElement(ElementName = "batteryInformation")]
				public string BatteryInformation { get; set; }
				[XmlElement(ElementName = "currencyCode")]
				public string CurrencyCode { get; set; }
				[XmlElement(ElementName = "languageInfo")]
				public string LanguageInfo { get; set; }
				[XmlElement(ElementName = "merchantId")]
				public string MerchantId { get; set; }
				[XmlElement(ElementName = "merhcantLocation")]
				public string MerhcantLocation { get; set; }
				[XmlElement(ElementName = "posConditionCode")]
				public string PosConditionCode { get; set; }
				[XmlElement(ElementName = "posDataCode")]
				public string PosDataCode { get; set; }
				[XmlElement(ElementName = "posEntryMode")]
				public string PosEntryMode { get; set; }
				[XmlElement(ElementName = "posGeoCode")]
				public string PosGeoCode { get; set; }
				[XmlElement(ElementName = "printerStatus")]
				public string PrinterStatus { get; set; }
				[XmlElement(ElementName = "terminalId")]
				public string TerminalId { get; set; }
				[XmlElement(ElementName = "terminalType")]
				public string TerminalType { get; set; }
				[XmlElement(ElementName = "transmissionDate")]
				public string TransmissionDate { get; set; }
				[XmlElement(ElementName = "uniqueId")]
				public string UniqueId { get; set; }

				public TerminalInformation() { }
			}

			[XmlRoot(ElementName = "emvData")]
			public class EmvData {
				[XmlElement(ElementName = "AmountAuthorized")]
				public string AmountAuthorized { get; set; }
				[XmlElement(ElementName = "AmountOther")]
				public string AmountOther { get; set; }
				[XmlElement(ElementName = "ApplicationInterchangeProfile")]
				public string ApplicationInterchangeProfile { get; set; }
				[XmlElement(ElementName = "atc")]
				public string Atc { get; set; }
				[XmlElement(ElementName = "Cryptogram")]
				public string Cryptogram { get; set; }
				[XmlElement(ElementName = "CryptogramInformationData")]
				public string CryptogramInformationData { get; set; }
				[XmlElement(ElementName = "CvmResults")]
				public string CvmResults { get; set; }
				[XmlElement(ElementName = "iad")]
				public string Iad { get; set; }
				[XmlElement(ElementName = "TransactionCurrencyCode")]
				public string TransactionCurrencyCode { get; set; }
				[XmlElement(ElementName = "TerminalVerificationResult")]
				public string TerminalVerificationResult { get; set; }
				[XmlElement(ElementName = "TerminalCountryCode")]
				public string TerminalCountryCode { get; set; }
				[XmlElement(ElementName = "TerminalType")]
				public string TerminalType { get; set; }
				[XmlElement(ElementName = "TerminalCapabilities")]
				public string TerminalCapabilities { get; set; }
				[XmlElement(ElementName = "TransactionDate")]
				public string TransactionDate { get; set; }
				[XmlElement(ElementName = "TransactionType")]
				public string TransactionType { get; set; }
				[XmlElement(ElementName = "UnpredictableNumber")]
				public string UnpredictableNumber { get; set; }
				[XmlElement(ElementName = "DedicatedFileName")]
				public string DedicatedFileName { get; set; }
			}

			[XmlRoot(ElementName = "track2")]
			public class Track2Root {
				[XmlElement(ElementName = "pan")]
				public string Pan { get; set; }
				[XmlElement(ElementName = "expiryMonth")]
				public string ExpiryMonth { get; set; }
				[XmlElement(ElementName = "expiryYear")]
				public string ExpiryYear { get; set; }
				[XmlElement(ElementName = "track2")]
				public string Track2 { get; set; }
			}

			[XmlRoot(ElementName = "cardData")]
			public class CardData {
				[XmlElement(ElementName = "cardSequenceNumber")]
				public string CardSequenceNumber { get; set; }
				[XmlElement(ElementName = "emvData")]
				public EmvData EmvData { get; set; }
				[XmlElement(ElementName = "track2")]
				public Track2Root Track2 { get; set; }
			}

			[XmlRoot(ElementName = "pinData")]
			public class PinData {
				[XmlElement(ElementName = "ksnd")]
				public string Ksnd { get; set; }
				[XmlElement(ElementName = "pinType")]
				public string PinType { get; set; }
				[XmlElement(ElementName = "ksn")]
				public string Ksn { get; set; }
				[XmlElement(ElementName = "pinBlock")]
				public string PinBlock { get; set; }
			}

			[XmlRoot(ElementName = "transferRequest")]
			public class TransferRequest {
				[XmlElement(ElementName = "terminalInformation")]
				public TerminalInformation TerminalInformation { get; set; }
				[XmlElement(ElementName = "cardData")]
				public CardData CardData { get; set; }
				[XmlElement(ElementName = "originalTransmissionDateTime")]
				public string OriginalTransmissionDateTime { get; set; }
				[XmlElement(ElementName = "stan")]
				public string Stan { get; set; }
				[XmlElement(ElementName = "fromAccount")]
				public string FromAccount { get; set; }
				[XmlElement(ElementName = "toAccount")]
				public string ToAccount { get; set; }
				[XmlElement(ElementName = "minorAmount")]
				public long MinorAmount { get; set; }
				[XmlElement(ElementName = "receivingInstitutionId")]
				public string ReceivingInstitutionId { get; set; }
				[XmlElement(ElementName = "pinData")]
				public PinData PinData { get; set; }
				[XmlElement(ElementName = "keyLabel")]
				public string KeyLabel { get; set; }
				[XmlElement(ElementName = "destinationAccountNumber")]
				public string DestinationAccountNumber { get; set; }
				[XmlElement(ElementName = "extendedTransactionType")]
				public string ExtendedTransactionType { get; set; }
				[XmlElement(ElementName = "retrievalReferenceNumber")]
				public string RetrievalReferenceNumber { get; set; }
			}
		}	

		public class Requery {
			[XmlRoot(ElementName = "terminalInformation")]
			public class TerminalInformation {
				[XmlElement(ElementName = "terminalId")]
				public string TerminalId { get; set; }
				[XmlElement(ElementName = "merchantId")]
				public string MerchantId { get; set; }
				[XmlElement(ElementName = "transmissionDate")]
				public string TransmissionDate { get; set; }
			}

			[XmlRoot(ElementName = "transactionRequeryRequest")]
			public class TransactionRequeryRequest {
				[XmlElement(ElementName = "applicationType")]
				public string ApplicationType { get; set; }
				[XmlElement(ElementName = "originalTransStan")]
				public string OriginalTransStan { get; set; }
				[XmlElement(ElementName = "originalMinorAmount")]
				public long OriginalMinorAmount { get; set; }
				[XmlElement(ElementName = "terminalInformation")]
				public TerminalInformation TerminalInformation { get; set; }
			}
		}
	}
}

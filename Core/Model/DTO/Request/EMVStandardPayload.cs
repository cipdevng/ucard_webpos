using Core.Model.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Request {
    public class EMVStandardPayload : ICloneable {
        public Channels? preferredChannel { get; set; }
        public long amount { get; set; }
        public string applicationInterchangeProfile { get; set; }
        public string atc { get; set; }
        public string cryptogram { get; set; }
        public string cryptogramInformationData { get; set; }
        public string cvmResults { get; set; }
        public string iad { get; set; }
        public string transactionCurrencyCode { get; set; }
        public string terminalVerificationResult { get; set; }
        public string terminalCountryCode { get; set; }
        public string terminalType { get; set; }
        public string terminalCapabilities { get; set; }
        public string transactionDate { get; set; }
        public string transDate { get; set; }
        public string transactionType { get; set; }
        public string unpredictableNumber { get; set; }
        public string dedicatedFileName { get; set; }
        public string terminalID { get; set; }
        public string cardSequenceNumber { get; set; }
        public string iccData { get; set; }
        public string stan { get; set; }
        public string rrn { get; set; }
        public string institutionCode { get; set; }
        public AccountType accountType { get; set; }
        public CardData CardData { get; set; }

        public object Clone() {
            var clone = JObject.FromObject(this).ToObject<EMVStandardPayload>();
            try {
                clone.CardData.pan = clone.CardData.pan[^4..].PadLeft(clone.CardData.pan.Length, '*');
                clone.CardData.pinBlock = "*".PadLeft(16, '*');
                clone.CardData.track2 = "*";
            } catch { }
            return clone;
        }
    }
    public class CardData {
        public string pan { get; set; }
        public string expiryMonth { get; set; }
        public string expiryYear { get; set; }
        public string track2 { get; set; }
        public string pinBlock { get; set; }
        public string expiry {
            get {
                return expiryYear + expiryMonth.PadLeft(2, '0');
            }
        }
    }
}

using Core.Application.Exceptions;
using Core.Model.DTO.Request;
using Core.Model.Enums;
using Core.Shared;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Entities {
    public class Transaction : BaseEntity {
        public string requestPayload { get; private set; }
        public string transID { get; private set; }
        public long transDate { get; private set; }
        public string deviceID { get; private set; }
        public string response { get; private set; }
        public string rawResponse { get; private set; }
        public string apiProfile { get; private set; }
        public Channels channel { get; private set; }
        public int status { get; private set; } = 0;
        public Transaction(){
        }
        public Transaction(EMVStandardPayload payload, string API) {
            string rrn = payload.rrn ?? Cryptography.CharGenerator.genID(12, Cryptography.CharGenerator.characterSet.NUMERIC);
            payload.rrn = rrn;
            if (payload.CardData == null)
                throw new InputError("Card data object was missing from request");
            cannotBeNullOrEmpty(payload.CardData.pan, payload.CardData.track2, payload.iccData, payload.terminalID);
            int year, month;
            bool parsedmonth = int.TryParse(payload.CardData.expiryMonth, out month);
            bool parsedyear = int.TryParse(payload.CardData.expiryYear, out year);
            if (!(parsedmonth && parsedyear))
                throw new InputError("Card Expiry was invalid");
            if(month > 12 || month < 1)
                throw new InputError("Card Expiry month was invalid");
            if (!string.IsNullOrEmpty(payload.CardData.pinBlock)) {
                if (!Utilities.isHexStr(payload.CardData.pinBlock))
                    throw new InputError("Invalid Pin block");
                if (payload.CardData.pinBlock.Length != 16)
                    throw new InputError("Invalid Pin block");
            }
            if (payload.amount < 1)
                throw new InputError("Invalid Amount");
            this.transID = rrn;
            var savablePayload = JObject.FromObject(payload.Clone()).ToString();
            this.requestPayload = savablePayload;
            this.transDate = Utilities.getTodayDate().unixTimestamp;
            this.deviceID = payload.terminalID;
            this.apiProfile = API;
            payload.institutionCode = payload.CardData.track2[0..6];
            this.channel = (Channels)payload.preferredChannel;
            validateICC(payload.iccData, payload);
        }
        public void chooseChannel(EMVStandardPayload payload) {
            if (payload.preferredChannel == default)
                return;
        }
        public void setTransactionStatus(string response, string rawResponse, int status) {
            this.response = response;
            this.rawResponse = rawResponse;
            this.status = status;
        }
        private void validateICC(string icc, EMVStandardPayload payload) {
            ICCDataDecoder decoder = new ICCDataDecoder(payload.iccData);
            payload.applicationInterchangeProfile = decoder.getCode(ICCDataTags.APPLICATION_INTERCHANGE_PROFILE);
            payload.atc = decoder.getCode(ICCDataTags.ATC);
            payload.cryptogram = decoder.getCode(ICCDataTags.APPLICATION_CRYPTOGRAM);
            payload.cryptogramInformationData = decoder.getCode(ICCDataTags.CID);
            payload.cvmResults = decoder.getCode(ICCDataTags.CVM_RESULT);
            payload.dedicatedFileName = decoder.getCode(ICCDataTags.DF_NAME);
            payload.iad = decoder.getCode(ICCDataTags.IAD);
            payload.terminalCapabilities = decoder.getCode(ICCDataTags.TERMINAL_CAPABILITIES);
            payload.terminalCountryCode = decoder.getCode(ICCDataTags.TCC);
            payload.terminalType = decoder.getCode(ICCDataTags.TERMINAL_TYPE);
            payload.terminalVerificationResult = decoder.getCode(ICCDataTags.TERMINAL_VERIFICATION_RESULT);
            payload.transactionCurrencyCode = decoder.getCode(ICCDataTags.T_CURRECNCY_CODE) ?? payload.terminalCountryCode;
            payload.transactionDate = decoder.getCode(ICCDataTags.TRANSACTION_DATE);
            payload.transactionType = decoder.getCode(ICCDataTags.TRANSACTION_TYPE);
            payload.unpredictableNumber = decoder.getCode(ICCDataTags.UNPREDICTABLE_NUMBER);
            payload.cardSequenceNumber = decoder.getCode(ICCDataTags.PAN_SN).PadLeft(3, '0');
        }
    }
}

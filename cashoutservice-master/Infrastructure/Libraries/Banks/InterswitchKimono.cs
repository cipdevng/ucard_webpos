using Core.Application.Exceptions;
using Core.Application.Interfaces.Bank;
using Core.Application.Interfaces.Cache;
using Core.Model.DTO.Configuration;
using Core.Model.DTO.Request;
using Core.Shared;
using Infrastructure.Abstraction.HTTP;
using Infrastructure.DTO;
using Microsoft.Extensions.Options;
using NetCore.AutoRegisterDi;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using static Core.Model.DTO.Response.KimonoPurchaseResponse;
using static Infrastructure.Model.KimonoRequest.Authentication;
using static Infrastructure.Model.KimonoRequest.Purchase;
using static Infrastructure.Model.KimonoRequest.Requery;

namespace Infrastructure.Libraries.Banks {
    [DoNotAutoRegister]
    public class InterswitchKimono : IEMVCardPayment {
        private readonly SystemVariables _sysVar;
        private readonly IHttpClient _request;
        private readonly ICacheService _cache;
        private SessionManager<SessionState<TokenPassportResponse>> _sessionState;
        public InterswitchKimono(IOptionsMonitor<SystemVariables> config, IHttpClient requests, ICacheService cache) {
            _sysVar = config.CurrentValue;
            _request = requests;
            _sessionState = new SessionManager<SessionState<TokenPassportResponse>>();
            _cache = cache;
        }
        private async Task<TokenPassportResponse> startSession(string terminalID) {
            if (_sessionState.exists(terminalID)) {
                var sessionObj = _sessionState.get(terminalID);
                if (!sessionObj.expired())
                    return sessionObj.state;
            }
            var requestData = new TokenPassportRequest {
                TerminalInformation = new Model.KimonoRequest.Authentication.TerminalInformation {
                    MerchantId = _sysVar.KimonoConfig.merchantID,
                    TerminalId = terminalID
                }
            };
            long expirySecs = 86400 - Utilities.getTodayDate().unixTimestamp % 86400;
            var reqString = Utilities.XMLSerializer<TokenPassportRequest>.serialize(requestData);
            var content = new StringContent(reqString, Encoding.UTF8, "application/xml");
            var responseObj = await _request.sendRequest(_sysVar.KimonoConfig.Endpoints.auth, HttpMethod.Post, content, new Dictionary<string, string>());
            if (responseObj.successful) {
                var responseDecoded = Utilities.XMLSerializer<TokenPassportResponse>.deserialize(responseObj.result);
                if (responseDecoded.ResponseCode != "00")
                    throw new LogicError(responseDecoded.ResponseMessage);
                responseDecoded.terminalID = terminalID;
                responseDecoded.dateCreated = Utilities.getTodayDate().unixTimestamp;
                _sessionState.add(terminalID,
                    new SessionState<TokenPassportResponse>(expirySecs) { state = responseDecoded });
                return responseDecoded;
            }
            throw new LogicError($"Remote server responded with an invalid status {responseObj.httpStatus}");
        }

        public async Task<TransferResponse> createPayment(EMVStandardPayload payload) {
            var token = await startSession(payload.terminalID);
            var track2 = new Track2Root {
                ExpiryMonth = payload.CardData.expiryMonth,
                ExpiryYear = payload.CardData.expiryYear,
                Pan = payload.CardData.pan,
                Track2 = payload.CardData.track2
            };
            var terminalInfo = new Model.KimonoRequest.Purchase.TerminalInformation {
                BatteryInformation = "-1",
                CurrencyCode = _sysVar.KimonoConfig.currencyCode,
                LanguageInfo = _sysVar.KimonoConfig.language,
                MerchantId = _sysVar.KimonoConfig.merchantID,
                MerhcantLocation = _sysVar.KimonoConfig.HQAddress,
                PosConditionCode = "00",
                PosDataCode = "510101511344101",
                PosEntryMode = "051",
                PosGeoCode = "00234000000000566",
                PrinterStatus = "1",
                TerminalId = payload.terminalID,
                TerminalType = payload.terminalType,
                TransmissionDate = payload.transDate,
                UniqueId = _sysVar.KimonoConfig.uniqueID
            };

            long koboAmount = payload.amount * 100;



            var emvData = new EmvData {
                AmountAuthorized = koboAmount.ToString().PadLeft(12, '0'),
                ApplicationInterchangeProfile = payload.applicationInterchangeProfile,
                AmountOther = "".PadLeft(12, '0'),
                Atc = payload.atc,
                Cryptogram = payload.cryptogram,
                CryptogramInformationData = payload.cryptogramInformationData,
                CvmResults = payload.cvmResults,
                DedicatedFileName = payload.dedicatedFileName,
                Iad = payload.iad,
                TerminalCapabilities = payload.terminalCapabilities,
                TerminalCountryCode = payload.terminalCountryCode[^3..],
                TerminalType = payload.terminalType,
                TerminalVerificationResult = payload.terminalVerificationResult,
                TransactionCurrencyCode = payload.transactionCurrencyCode[^3..],
                TransactionDate = payload.transactionDate,
                TransactionType = payload.transactionType,
                UnpredictableNumber = payload.unpredictableNumber,
            };
            var cardData = new Model.KimonoRequest.Purchase.CardData {
                EmvData = emvData,
                CardSequenceNumber = payload.cardSequenceNumber.PadLeft(3, '0'),
                Track2 = track2
            };
            PinData? pinData = null;
            if(!string.IsNullOrEmpty(payload.CardData.pinBlock)) {
                pinData = new PinData {
                };
                string serial = Cryptography.CharGenerator.genID(4, Cryptography.CharGenerator.characterSet.NUMERIC);
                string hex = int.Parse(serial).ToString("X").PadLeft(4, '0');
                string ksn = _sysVar.KimonoConfig.KSN + serial.ToString();
                string ksnHex = _sysVar.KimonoConfig.KSN + hex;
                var sessionKey = getSessionKey(_sysVar.KimonoConfig.IPEK, ksn);
                var duktp = InterswitchKimono.desEncryptDukpt(sessionKey, payload.CardData.pan, payload.CardData.pinBlock);
                pinData.Ksnd = _sysVar.KimonoConfig.KSNd;
                pinData.Ksn = ksnHex;
                pinData.PinBlock = duktp;
                pinData.PinType = "Dukpt";
            }
            var transferReq = new TransferRequest {
                TerminalInformation = terminalInfo,
                CardData = cardData,
                DestinationAccountNumber = _sysVar.KimonoConfig.destinationAccountNumber,
                ExtendedTransactionType = _sysVar.KimonoConfig.extendedTransactionType,
                FromAccount = ((int)payload.accountType).ToString().PadLeft(2, '0'),
                KeyLabel = _sysVar.KimonoConfig.keyLabel,
                MinorAmount = payload.amount,
                OriginalTransmissionDateTime = payload.transDate,
                PinData = pinData,
                ReceivingInstitutionId = _sysVar.KimonoConfig.receivingInstitutionId,
                ToAccount = "",
                Stan = payload.stan,
                RetrievalReferenceNumber = payload.rrn
            };            
            var reqString = Utilities.XMLSerializer<TransferRequest>.serialize(transferReq);
            var content = new StringContent(reqString, Encoding.UTF8, "application/xml");
            var header = new Dictionary<string, string>();
            header.Add("Authorization", $"Bearer {token.Token}");
            var responseObj = await _request.sendRequest(_sysVar.KimonoConfig.Endpoints.purchase, HttpMethod.Post, content, header);
            TransferResponse responseDecoded;
            string rawResponse = $"SuccessResponse:>\n{responseObj.result}\nFailureResponse:>\n{responseObj.error}";
            if (!(_cache is null))
                await _cache.addWithKey(payload.rrn, rawResponse, 60);
            string respDataStr = string.Empty;
            if (responseObj.successful) {
                respDataStr = responseObj.result;                                
            } else {
                if (responseObj.httpStatus == 401) {
                    await startSession(payload.terminalID);
                    throw new Exception("Could not start device session. You can retry later or immediately");
                }
                respDataStr = responseObj.error;
            }
            if (respDataStr.StartsWith("{")) {
                responseDecoded = JObject.Parse(respDataStr).ToObject<TransferResponse>();
            } else {
                responseDecoded = Utilities.XMLSerializer<TransferResponse>.deserialize(respDataStr);
            }
            if (responseDecoded.responseCode == "500")
                throw new ServiceError(responseDecoded.responseMessage);
            responseDecoded.terminalID = payload.terminalID;
            responseDecoded.makedPAN = payload.CardData.pan[^4..].PadLeft(payload.CardData.pan.Length, '*');
            responseDecoded.ReferenceNumber = payload.rrn;
            responseDecoded.cardExpiry = payload.CardData.expiry;
            responseDecoded.merchantID = _sysVar.KimonoConfig.merchantID;
            responseDecoded.MerchantAddress = _sysVar.KimonoConfig.HQAddress;
            responseDecoded.MerchantName = _sysVar.KimonoConfig.HQAddress;
            responseDecoded.amount = payload.amount;
            responseDecoded.TransactionChannelName = "KIMONO";
            return responseDecoded;
        }

        public async Task<TransactionRequeryResponse> requery(EMVStandardPayload payload) {
            var reqObj = new TransactionRequeryRequest {
                ApplicationType = "gTransfer",
                OriginalMinorAmount = payload.amount,
                OriginalTransStan = payload.stan,
                TerminalInformation = new Model.KimonoRequest.Requery.TerminalInformation {
                    MerchantId = _sysVar.KimonoConfig.merchantID,
                    TerminalId = payload.terminalID,
                    TransmissionDate = payload.transactionDate
                }
            };
            var reqString = Utilities.XMLSerializer<TransactionRequeryRequest>.serialize(reqObj);
            var content = new StringContent(reqString, Encoding.UTF8, "application/xml");
            var header = new Dictionary<string, string>();
            var responseObj = await _request.sendRequest(_sysVar.KimonoConfig.Endpoints.purchase, HttpMethod.Post, content, header);
            TransactionRequeryResponse responseDecoded;
            if (responseObj.successful) {
                responseDecoded = Utilities.XMLSerializer<TransactionRequeryResponse>.deserialize(responseObj.result);
            } else {
                responseDecoded = Utilities.XMLSerializer<TransactionRequeryResponse>.deserialize(responseObj.error);
            }
            return responseDecoded;
        }

        public static string getSessionKey(string IPEK, string KSN) {
            string initialIPEK = IPEK, ksn = KSN.PadLeft(20, '0');
            string sessionkey = "";
            //Get ksn with a zero counter by ANDing it with FFFFFFFFFFFFFFE00000
            string newKSN = XORorANDorORfuction(ksn, "0000FFFFFFFFFFE00000", "&");
            string counterKSN = ksn.Substring(ksn.Length - 5).PadLeft(16, '0');
            //get the number of binaray associated with the counterKSN number
            string newKSNtoleft16 = newKSN.Substring(newKSN.Length - 16);
            string counterKSNbin = Convert.ToString(Convert.ToInt32(counterKSN), 2);
            int count = Convert.ToString(Convert.ToInt32(counterKSN), 2)
            .Replace("0", "").Length;
            string binarycount = counterKSNbin;
            for (int i = 0; i < counterKSNbin.Length; i++) {
                int len = binarycount.Length; string result = "";
                if (binarycount.Substring(0, 1) == "1") {
                    result = "1".PadRight(len, '0');
                    binarycount = binarycount.Substring(1);
                } else { binarycount = binarycount.Substring(1); continue; }
                string counterKSN2 = Convert.ToInt32(result, 2).ToString("X2").PadLeft(16,
               '0');
                string newKSN2 = XORorANDorORfuction(newKSNtoleft16, counterKSN2, "|");
                sessionkey = BlackBoxLogic(newKSN2, initialIPEK);
                newKSNtoleft16 = newKSN2;
                initialIPEK = sessionkey;
            }
            return XORorANDorORfuction(sessionkey, "00000000000000FF00000000000000FF", "^");
        }

        public static string BlackBoxLogic(string ksn, string ipek) {
            if (ipek.Length < 32) {
                string msg = XORorANDorORfuction(ipek, ksn, "^");
                string desreslt = DesEncrypt(msg, ipek);
                string rsesskey = XORorANDorORfuction(desreslt, ipek, "^");
                return rsesskey;
            }
            string current_sk = ipek;
            string ksn_mod = ksn;
            string leftIpek = XORorANDorORfuction(current_sk,
           "FFFFFFFFFFFFFFFF0000000000000000", "&").Remove(16);
            string rightIpek = XORorANDorORfuction(current_sk,
           "0000000000000000FFFFFFFFFFFFFFFF", "&").Substring(16);
            string message = XORorANDorORfuction(rightIpek, ksn_mod, "^");
            string desresult = DesEncrypt(message, leftIpek);
            string rightSessionKey = XORorANDorORfuction(desresult, rightIpek, "^");
            string resultCurrent_sk = XORorANDorORfuction(current_sk,
           "C0C0C0C000000000C0C0C0C000000000", "^");
            string leftIpek2 = XORorANDorORfuction(resultCurrent_sk,
           "FFFFFFFFFFFFFFFF0000000000000000", "&").Remove(16);
            string rightIpek2 = XORorANDorORfuction(resultCurrent_sk,
           "0000000000000000FFFFFFFFFFFFFFFF", "&").Substring(16);
            string message2 = XORorANDorORfuction(rightIpek2, ksn_mod, "^");
            string desresult2 = DesEncrypt(message2, leftIpek2);
            string leftSessionKey = XORorANDorORfuction(desresult2, rightIpek2, "^");
            string sessionkey = leftSessionKey + rightSessionKey;
            return sessionkey;
        }

        public static byte[] StringToByteArray(String hex) {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string DesEncrypt(string desData, string key) {
            byte[] dData = StringToByteArray(desData);
            byte[] keyData = StringToByteArray(key);
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            tdes.Key = keyData;
            tdes.Padding = PaddingMode.None;
            tdes.Mode = CipherMode.ECB;
            ICryptoTransform transform = tdes.CreateEncryptor();
            byte[] result = transform.TransformFinalBlock(dData, 0, dData.Length);
            return BitConverter.ToString(result).Replace("-", "");
        }

        public static string encryptPinBlock(string pan, string pin) {
            pan = pan.Substring(pan.Length - 13, 12).PadLeft(16, '0');
            pin = pin.Length.ToString("X2") + pin.PadRight(16, 'F');
            return XORorANDorORfuction(pan, pin, "^");
        }

        public static string XORorANDorORfuction(string valueA, string valueB, string symbol = "|") {
            char[] a = valueA.ToCharArray();
            char[] b = valueB.ToCharArray();
            string result = "";
            for (int i = 0; i < a.Length; i++) {
                if (symbol == "|") result += (Convert.ToInt32(a[i].ToString(), 16) |
              Convert.ToInt32(b[i].ToString(), 16)).ToString("x").ToUpper();
                else if (symbol == "^") result += (Convert.ToInt32(a[i].ToString(), 16) ^
               Convert.ToInt32(b[i].ToString(), 16)).ToString("x").ToUpper();
                else result += (Convert.ToInt32(a[i].ToString(), 16) &
               Convert.ToInt32(b[i].ToString(), 16)).ToString("x").ToUpper();
            }
            return result;
        }

        public static string desEncryptDukpt(string workingKey, string pan, string clearPin) {
            //string ePIN = encryptPinBlock(pan, clearPin);
            string pinblock = XORorANDorORfuction(workingKey, clearPin, "^");
            byte[] dData = StringToByteArray(pinblock);
            byte[] keyData = StringToByteArray(workingKey);
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            tdes.Key = keyData;
            tdes.Padding = PaddingMode.None;
            tdes.Mode = CipherMode.ECB;
            ICryptoTransform transform = tdes.CreateEncryptor();
            byte[] result = transform.TransformFinalBlock(dData, 0, dData.Length);
            return XORorANDorORfuction(workingKey, BitConverter.ToString(result).Replace("-",
           ""), "^");
        }

        public Task<double> getBalance(EMVStandardPayload payload) {
            throw new NotImplementedException();
        }

        public Task<double> processRefund(EMVStandardPayload payload) {
            throw new NotImplementedException();
        }
    }
}

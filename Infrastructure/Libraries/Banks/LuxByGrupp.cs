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

namespace Infrastructure.Libraries.Banks {
    [DoNotAutoRegister]
    public class LuxByGrupp : IEMVCardPayment {
        private readonly SystemVariables _sysVar;
        private readonly IHttpClient _request;
        private readonly LuxConfig _luxConfig;
        private SessionManager<SessionState<SessionData>> _sessionState;
        private ICacheService _cache;
        public LuxByGrupp(IOptionsMonitor<SystemVariables> config, IHttpClient requests, ICacheService cache) {
            _sysVar = config.CurrentValue;
            _request = requests;
            _luxConfig = _sysVar.LuxConfig;
            _sessionState = new SessionManager<SessionState<SessionData>>();
            _cache = cache;
        }

        public LuxByGrupp(SystemVariables config, IHttpClient requests, ICacheService cache = null) {
            _sysVar = config;
            _request = requests;
            _luxConfig = _sysVar.LuxConfig;
            _cache = cache;
        }

        private async Task<SessionData> getSession(string serialNumber, string stan) {
            if (_sessionState.exists(serialNumber)) {
                var sessionObj = _sessionState.get(serialNumber);
                if (!sessionObj.expired())
                    return sessionObj.state;
            }
            long expirySecs = 86400 - Utilities.getTodayDate().unixTimestamp % 86400;
            var data = JObject.FromObject(new { serialNumber = serialNumber, stan = stan, onlyAccountInfo = true }).ToString();
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            var authenticationString = $"{_luxConfig.basicUsername}:{_luxConfig.basicPassword}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(authenticationString));
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");
            var responseObj = await _request.sendRequest(_luxConfig.Endpoints.startSession, HttpMethod.Post, content, headers);
            if(!responseObj.successful)
                if(string.IsNullOrEmpty(responseObj.result))
                    throw new LogicError($"Remote server responded with an invalid status {responseObj.httpStatus}");
            var responseObject = JObject.Parse(responseObj.result).ToObject<ResponseBase<SessionData>>();
            if(responseObject == null)
                throw new LogicError($"Response could not be parsed {responseObj.httpStatus}");
            if (!responseObject.status)
                throw new LogicError($"{responseObject.message}");
            responseObject.data.stan = stan;
            _sessionState.add(serialNumber,
                new SessionState<SessionData>(expirySecs) { state = responseObject.data });
            return responseObject.data;
        }

        public async Task<TransferResponse> createPayment(EMVStandardPayload payload) {
            string stan = Cryptography.CharGenerator.genID(6, Cryptography.CharGenerator.characterSet.NUMERIC);
            var session = await getSession(payload.terminalID, stan);
            var payloadObj = new { pan = payload.CardData.pan, stan = stan, rrn = payload.rrn, amount = payload.amount, iccData = payload.iccData, track2Data  = payload.CardData.track2, postDataCode  = _luxConfig.postDataCode, cardExpiryDate = payload.CardData.expiryYear+payload.CardData.expiryMonth, acquiringInstitutionalCode = payload.institutionCode, sequenceNumber = payload.cardSequenceNumber.PadLeft(3, '0'), pin = payload.CardData.pinBlock, type = "CARD", accountType = payload.accountType.ToString() };
            string jdata = JObject.FromObject(payloadObj).ToString(Newtonsoft.Json.Formatting.None);
            var data = tdesEncrypt(jdata, session.sessionId);
            var authenticationString = $"{_luxConfig.basicUsername}:{_luxConfig.basicPassword}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(authenticationString));
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("terminalId", $"{session.terminalId}");
            headers.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");
            var content = new StringContent(data, Encoding.UTF8, "text/plain");
            string url = _luxConfig.Endpoints.transaction;
            var responseObj = await _request.sendRequest(url, HttpMethod.Post, content, headers);
            string rawResponse = $"SuccessResponse:>\n{responseObj.result}\nFailureResponse:>\n{responseObj.error}";
            if (!(_cache is null))
                await _cache.addWithKey(payload.rrn, rawResponse, 60);
            if (!responseObj.successful) {
                if (string.IsNullOrEmpty(responseObj.error))
                    throw new LogicError($"Remote server responded with an invalid status {responseObj.httpStatus}");
                var responseObject = JObject.Parse(responseObj.error).ToObject<ResponseBase<string>>();
                return new TransferResponse {
                    Field39 = "-1",
                    ReferenceNumber = payload.rrn,
                    Description = responseObject.message
                };
            } else {
                var responseObject = JObject.Parse(responseObj.result).ToObject<ResponseBase<string>>();
                string rawData = tdesDecrypt(responseObject.data, session.sessionId).Trim('\u0001').Trim('\u0002').Trim('\a');
                int lastInd = rawData.LastIndexOf("}")+1;
                rawData = rawData[0..lastInd];
                var resData = JObject.Parse(rawData).ToObject<PaymentResponse>();
                return new TransferResponse {
                    Field39 = resData.responseCode,
                    ReferenceNumber = payload.rrn,
                    Description = resData.description,
                    terminalID = session.terminalId
                };
            }            
        }

        public async Task<double> getBalance(EMVStandardPayload payload) {
            string stan = Cryptography.CharGenerator.genID(6, Cryptography.CharGenerator.characterSet.NUMERIC);
            var session = await getSession(payload.terminalID, stan);
            var payloadObj = new { pan = payload.CardData.pan, stan = stan, rrn = payload.rrn, amount = payload.amount, iccData = payload.iccData, track2Data = payload.CardData.track2, postDataCode = _luxConfig.postDataCode, cardExpiryDate = payload.CardData.expiryMonth + payload.CardData.expiryYear, acquiringInstitutionalCode = payload.institutionCode, sequenceNumber = payload.cardSequenceNumber, pin = payload.CardData.pinBlock, type = "CARD", accountType = payload.accountType.ToString() };
            var data = tdesEncrypt(JObject.FromObject(payloadObj).ToString(), session.sessionId);
            var authenticationString = $"{_luxConfig.basicUsername}:{_luxConfig.basicPassword}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(authenticationString));
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("terminalId", $"{session.terminalId}");
            headers.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");
            var content = new StringContent(data, Encoding.UTF8, "application/text");
            string url = _luxConfig.Endpoints.getBalance;
            var responseObj = await _request.sendRequest(url, HttpMethod.Post, content, headers);
            if (!responseObj.successful)
                if (string.IsNullOrEmpty(responseObj.result))
                    throw new LogicError($"Remote server responded with an invalid status {responseObj.httpStatus}");
            var responseObject = JObject.Parse(responseObj.result).ToObject<ResponseBase<string>>();
            string rawData = tdesDecrypt(responseObject.data, session.sessionId).Trim('\u0001'); ;
            var resData = JObject.Parse(rawData).ToObject<BalanceResponse>();
            return resData.cardBalance;
        }

        public async Task<double> processRefund(EMVStandardPayload payload) {
            throw new NotImplementedException();
        }

        public async Task<TransactionRequeryResponse> requery(EMVStandardPayload payload) {
            string stan = Cryptography.CharGenerator.genID(6, Cryptography.CharGenerator.characterSet.NUMERIC);
            var session = await getSession(payload.terminalID, stan);
            var authenticationString = $"{_luxConfig.basicUsername}:{_luxConfig.basicPassword}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(authenticationString));
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("terminalId", $"{session.terminalId}");
            headers.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");
            string url = $"{_luxConfig.Endpoints.query}?rrn={payload.rrn}";
            var responseObj = await _request.sendRequest(url, HttpMethod.Get, null, headers);
            if (!responseObj.successful)
                if (string.IsNullOrEmpty(responseObj.result))
                    throw new LogicError($"Remote server responded with an invalid status {responseObj.httpStatus}");
            var responseObject = JObject.Parse(responseObj.result).ToObject<ResponseBase<string>>();
            if (!responseObject.status)
                throw new LogicError($"{responseObject.message}");
            string rawData = tdesDecrypt(responseObject.data, session.sessionId).Trim('\u0001'); ;
            var resData = JObject.Parse(rawData).ToObject<RequeryResponse>();
            if (resData.total < 1)
                return null;
            return new TransactionRequeryResponse { ReferenceNumber = resData.list[0].rrn, Stan = "", Field39 = "00", Description = "" };
        }

        public string tdesDecrypt(string raw, string key) {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] rawBytes = Convert.FromBase64String(raw);
            using (MD5 md5 = MD5.Create()) {
                byte[] hashBytes = md5.ComputeHash(keyBytes);
                byte[] finalKeyBytes = new byte[24];
                int j = 0;
                for(int i=0; i < 24; i++) {
                    if (j >= 16)
                        j = 0;
                    finalKeyBytes[i] = hashBytes[j];
                    j++;
                }
                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = finalKeyBytes;
                tdes.Padding = PaddingMode.None;
                tdes.Mode = CipherMode.CBC;
                tdes.IV = new byte[8];
                ICryptoTransform transform = tdes.CreateDecryptor();
                byte[] result = transform.TransformFinalBlock(rawBytes, 0, rawBytes.Length);
                return UTF8Encoding.UTF8.GetString(result);
            }
        }

        public string tdesEncrypt(string raw, string key) {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] rawBytes = Encoding.UTF8.GetBytes(raw);
            using (MD5 md5 = MD5.Create()) {
                byte[] hashBytes = md5.ComputeHash(keyBytes);
                byte[] finalKeyBytes = new byte[24];
                int j = 0;
                for (int i = 0; i < 24; i++) {
                    if (j >= 16)
                        j = 0;
                    finalKeyBytes[i] = hashBytes[j];
                    j++;
                }
                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = finalKeyBytes;
                tdes.Padding = PaddingMode.PKCS7;
                tdes.Mode = CipherMode.CBC;
                tdes.IV = new byte[8];
                ICryptoTransform transform = tdes.CreateEncryptor();
                byte[] result = transform.TransformFinalBlock(rawBytes, 0, rawBytes.Length);
                return System.Convert.ToBase64String(result);
            }
        }

        public class SessionData {
            public string sessionId { get; set; }
            public string terminalId { get; set; }
            public string merchantId { get; set; }
            public string stan { get; set; }
        }
        public class PaymentResponse {
            public string responseCode { get; set; }
            public string description { get; set; }
        }
        public class BalanceResponse {
            public string responseCode { get; set; }
            public string description { get; set; }
            public double cardBalance { get; set; }
        }
        public class RequeryData {
            public string reference { get; set; }
            public double amount { get; set; }
            public double serviceFee { get; set; }
            public string transactionType { get; set; }
            public string user { get; set; }
            public string rrn { get; set; }
            public string pan { get; set; }
            public string timeCreated { get; set; }
        }
        public class RequeryResponse {
            public List<RequeryData> list { get; set; }
            public int page { get; set; }
            public int total { get; set; }
        }
        public class ResponseBase<T> {
            public bool status { get; set; }
            public T data { get; set; }
            public string message { get; set; }
        }
    }
}

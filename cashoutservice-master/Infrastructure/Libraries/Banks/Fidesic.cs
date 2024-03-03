using Core.Application.Exceptions;
using Core.Application.Interfaces.Bank;
using Core.Application.Interfaces.Cache;
using Core.Model.DTO.Configuration;
using Core.Model.DTO.Request;
using Core.Shared;
using Infrastructure.Abstraction.HTTP;
using Infrastructure.Abstraction.Socket;
using Infrastructure.DTO;
using Infrastructure.DTO.ISOTemplates;
using Microsoft.Extensions.Options;
using NetCore.AutoRegisterDi;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using static Core.Model.DTO.Response.KimonoPurchaseResponse;

namespace Infrastructure.Libraries.Banks {
    [DoNotAutoRegister]
    public class Fidesic : IEMVCardPayment {
        private readonly SystemVariables _sysVar;
        private readonly FidesicConfiguration _config;
        private SessionManager<SessionState<FidesicConfig>> _sessionState;
        private readonly IHttpClient _request;
        private AccessToken accessToken = null;
        private ICacheService _cache;
        private readonly IWebSocketManager _socket;

        public Fidesic(SystemVariables sysvar, IHttpClient requests, ICacheService cache = null) {
            this._sysVar = sysvar;
            this._config = _sysVar.FidesicConfig;
            _sessionState = new SessionManager<SessionState<FidesicConfig>>();
            this._request = requests;
            _cache = cache;
        }

        public Fidesic(IOptionsMonitor<SystemVariables> sysvar, IHttpClient requests, ICacheService cache, IWebSocketManager manager) {
            this._sysVar = sysvar.CurrentValue;
            this._config = _sysVar.FidesicConfig;
            _sessionState = new SessionManager<SessionState<FidesicConfig>>();
            this._request = requests;
            this._cache = cache;
            _socket = manager;
        }

        private async Task getToken() {
            if (accessToken != null)
                if (!accessToken.fetchNewToken())
                    return;
            var ob = JObject.FromObject(new { clientId = _config.clientID, clientSecret = _config.clientSecret }).ToString();
            var content = new StringContent(ob, Encoding.UTF8, "application/json");
            var headers = new Dictionary<string, string>();
            var responseObj = await _request.sendRequest(_config.Endpoints.gettoken, HttpMethod.Post, content, headers);
            if (responseObj.successful) {
                if (string.IsNullOrEmpty(responseObj.result))
                    throw new ServiceError("Invalid Request. The service returned an invalid response");
                var data = JObject.Parse(responseObj.result).ToObject<AccessToken>();
                this.accessToken = data;
                return;
            }
            throw new ServiceError($"Remote server responded with an invalid status {responseObj.httpStatus}");
        }

        private async Task<FidesicConfig> keyExchange(string terminalID) {
            await getToken();
            long toDayEnd = 580; //86400 - Utilities.getTodayDate().unixTimestamp % 86400;
            if (_sessionState.exists(terminalID)) {
                var sessionObj = _sessionState.get(terminalID);
                if (!sessionObj.expired()) {
                    return sessionObj.state;
                }
            }
            string serial = Cryptography.CharGenerator.genID(19, Cryptography.CharGenerator.characterSet.ALPHA_NUMERIC_CASE);
            var req = JObject.FromObject(new {
                terminalId = terminalID,
                terminalSerial = serial
            }).ToString();
            var content = new StringContent(req, Encoding.UTF8, "application/json");
            var headers = new Dictionary<string, string>();
            headers.Add("Authorization", $"Bearer {accessToken.accessToken}");
            var responseObj = await _request.sendRequest(_config.Endpoints.keyRequest, HttpMethod.Post, content, headers);
            if (responseObj.successful) {
                var dataObj = JObject.Parse(responseObj.result).ToObject<ResponseData<FidesicConfig>>();
                var data = dataObj.responseData;
                if (data == null)
                    throw new ServiceError("Invalid Request. The service returned an invalid response");
                data.terminalId = terminalID;
                data.terminalSerial = serial;
                _sessionState.add(terminalID,
                    new SessionState<FidesicConfig>(toDayEnd) { state = data });
                return data;
            }
            throw new ServiceError($"Remote server responded with an invalid status {responseObj.httpStatus}");
        }

        public async Task<TransferResponse> createPayment(EMVStandardPayload payload) {
            var dateObj = Utilities.getTransactionDate();
            var config = await keyExchange(payload.terminalID);
            string pCode = $"00{((int)payload.accountType).ToString().PadLeft(2, '0')}00";
            FidesicPaymentRequest request = new FidesicPaymentRequest();
            request.add(2, payload.CardData.pan);
            request.add(3, pCode);
            request.add(4, payload.amount.ToString().PadLeft(12, '0'));
            request.add(7, dateObj.transmDate);
            request.add(11, dateObj.time);
            request.add(12, dateObj.time);
            request.add(13, dateObj.day);
            request.add(14, payload.CardData.expiry);
            request.add(18, config.terminalParameter.merchantType);
            request.add(22, "051");
            request.add(23, "001");
            request.add(25, _config.posCondition);
            request.add(26, _config.posPinCaptureCode);
            request.add(28, "C00000000");
            request.add(32, payload.institutionCode);
            request.add(35, payload.CardData.track2);
            request.add(37, payload.rrn);
            request.add(40, "221");
            request.add(41, payload.terminalID);
            request.add(42, config.terminalParameter.acceptorId);
            request.add(43, config.terminalParameter.acceptorName);
            request.add(49, config.terminalParameter.currencyCode);
            if (!string.IsNullOrEmpty(payload.CardData.pinBlock)) {
                request.add(52, getEncKey(payload.CardData.pinBlock, config.pinKey));
            }
            request.add(55, Utilities.reconstructICC(payload.iccData));
            request.add(123, _config.posDataCode);
            request.add(128, Cryptography.CharGenerator.genID(64, Cryptography.CharGenerator.characterSet.HEX_STRING));
            request.add("sessionId", config.sessionKey);
            request.add("terminalId", config.terminalId);
            request.add("terminalSerial", config.terminalSerial);
            var reqstr = JObject.FromObject(request).ToString();
            var content = new StringContent(reqstr, Encoding.UTF8, "application/json");
            var headers = new Dictionary<string, string>();
            headers.Add("Authorization", $"Bearer {accessToken.accessToken}");
            var responseObj = await _request.sendRequest(_config.Endpoints.purchase, HttpMethod.Post, content, headers);
            string rawResponse = $"SuccessResponse:>\n{responseObj.result}\nFailureResponse:>\n{responseObj.error}";
            if(!(_cache is null))
                await _cache.addWithKey(payload.rrn, rawResponse, 60);
            ResponseData<TransferResponse> responseDecoded;
            if (responseObj.successful) {
                if (string.IsNullOrEmpty(responseObj.result))
                    throw new ServiceError("Invalid Request. The service returned an invalid response");
                responseDecoded = JObject.Parse(responseObj.result).ToObject<ResponseData<TransferResponse>>();
                responseDecoded.responseData.Description = responseDecoded.message;
            } else {
                if(string.IsNullOrEmpty(responseObj.error))
                    throw new ServiceError($"Remote server responded with an invalid status {responseObj.httpStatus}");
                responseDecoded = JObject.Parse(responseObj.error).ToObject<ResponseData<TransferResponse>>();
            }
            if(!string.IsNullOrEmpty(responseDecoded.responseData.tmsResponse)) {
                try {
                    var respData = JObject.Parse(responseDecoded.responseData.tmsResponse).ToObject<ResponseData<ResponseInformation>>();
                    responseDecoded.responseData.terminalID = respData.responseData.TerminalId;
                    responseDecoded.responseData.MerchantName = respData.responseData.MerchantName;
                    responseDecoded.responseData.merchantID = respData.responseData.MerchantId;
                    responseDecoded.responseData.AuthId = respData.responseData.AuthCode;
                    responseDecoded.responseData.ProcessingCode = respData.responseData.ProcessingCode;
                    responseDecoded.responseData.makedPAN = respData.responseData.Pan;
                    responseDecoded.responseData.MerchantAddress = respData.responseData.MerchantAddress;
                    responseDecoded.responseData.cardExpiry = respData.responseData.CardExpiry;
                    responseDecoded.responseData.ReferenceNumber = respData.responseData.RetrievalReferenceNumber;
                    responseDecoded.responseData.Stan = respData.responseData.Stan;
                } catch { }
            }
            return responseDecoded.responseData;
        }

        public Task<TransactionRequeryResponse> requery(EMVStandardPayload payload) {
            throw new NotImplementedException();
        }

        public Task<double> getBalance(EMVStandardPayload payload) {
            throw new NotImplementedException();
        }

        public Task<double> processRefund(EMVStandardPayload payload) {
            throw new NotImplementedException();
        }

        private async Task<bool> publishToSocket(string data) {
            if (_socket is null)
                return false;
            await _socket.sendMessage(data);
            return true;
        }

        public class FidesicConfig {
            public string masterKey { get; set; }
            public string sessionKey { get; set; }
            public string pinKey { get; set; }
            public string terminalId { get; set; }
            public string terminalSerial { get; set; }
            public FidesicDeviceParameter terminalParameter { get; set; }
        }
        private string getEncKey(string key, string pinKey) {
            byte[] keyRaw = Utilities.stringToByteArray(key);
            byte[] pinkeyArr = Utilities.stringToByteArray(pinKey);
            var stringEnc = TripleDES.encrypt(keyRaw, pinkeyArr);
            Debug.WriteLine($"Encrypting {key} with {BitConverter.ToString(pinkeyArr).Replace("-", "")} Yield {BitConverter.ToString(stringEnc).Replace("-", "")}");
            var stringDec = TripleDES.decrypt(stringEnc, pinkeyArr);
            Debug.WriteLine($"Reversed enc Yields {BitConverter.ToString(stringDec)}");
            return BitConverter.ToString(stringEnc).Replace("-", "");
        }

        public class FidesicDeviceParameter {
            public string acceptorId { get; set; }
            public string merchantType { get; set; }
            public string acceptorName { get; set; }
            public string currencyCode { get; set; }
            public string responseCode { get; set; }
        }

        private const int Keysize = 128;
        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public string encrypt(string plainText, string key) {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.
            var saltStringBytes = Generate128BitsOfRandomEntropy();
            var ivStringBytes = Generate128BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(key, saltStringBytes, DerivationIterations)) {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged()) {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes)) {
                        using (var memoryStream = new MemoryStream()) {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor,
                            CryptoStreamMode.Write)) {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes,                                the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes =
                                cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public string decrypt(string cipherText, string key) {
            // Get the complete stream of bytes that represent:
            // [16 bytes of Salt] + [16 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize/8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText  string.
var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) *
2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();
            using (var password = new Rfc2898DeriveBytes(key, saltStringBytes, DerivationIterations)) {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged()) {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes)) {
                        using (var memoryStream = new MemoryStream(cipherTextBytes)) {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor,
                            CryptoStreamMode.Read)) {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0,
                                plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0,
                                decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate128BitsOfRandomEntropy() {
            var randomBytes = new byte[16]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider()) {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        public class AccessToken {
            public string accessToken { get; set; }
            public string refreshToken { get; set; }
            public string clientId { get; set; }
            public string businessId { get; set; }
            public string role { get; set; }
            public string partnerCode { get; set; }
            public long dateCreated { get; set; } = Utilities.getTodayDate().unixTimestamp;
            public bool fetchNewToken() {
                if (Utilities.getTodayDate().unixTimestamp - dateCreated > 600)
                    return true;
                return false;
            }
        }
    }
 
    public class ResponseData<T> {
        public bool status { get; set; }
        public T responseData { get; set; }
        public string message { get; set; }
        public string responseCode { get; set; }
    }
    public class ResponseInformation {
        public string Id { get; set; }
        public string MerchantName { get; set; }
        public string MerchantAddress { get; set; }
        public string MerchantId { get; set; }
        public string TerminalId { get; set; }
        public string TransactionDate { get; set; }
        public string Stan { get; set; }
        public string Pan { get; set; }
        public string RetrievalReferenceNumber { get; set; }
        public string ProcessingCode { get; set; }
        public string ResponseMessage { get; set; }
        public string AuthCode { get; set; }
        public string Reference { get; set; }
        public string CardExpiry { get; set; }
    }
}

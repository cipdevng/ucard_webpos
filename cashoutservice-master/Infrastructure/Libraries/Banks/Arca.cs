using Core.Application.Exceptions;
using Core.Application.Interfaces.Bank;
using Core.Application.Interfaces.Cache;
using Core.Model.DTO.Configuration;
using Core.Model.DTO.Request;
using Core.Shared;
using Infrastructure.Abstraction.HTTP;
using Infrastructure.Abstraction.Socket;
using Infrastructure.DTO;
using Infrastructure.DTO.Enums;
using Infrastructure.DTO.ISOTemplates.UP;
using Infrastructure.ISOTemplates.UP;
using Infrastructure.Libraries.HTTP;
using Iso8583CS;
using Microsoft.Extensions.Options;
using NetCore.AutoRegisterDi;
using NetCore8583;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using static Core.Model.DTO.Response.KimonoPurchaseResponse;

namespace Infrastructure.Libraries.Banks {
    [DoNotAutoRegister]
    public class ArcaImpl : IEMVCardPayment {
        private readonly SystemVariables _sysVar;
        private readonly ArcaConfig _config;
        private SessionManager<SessionState<ArcaSessionData>> _sessionState;
        private Dictionary<Keys, string> processingCodes;
        private readonly IHttpClient _request;
        private ICacheService _cache;
        private readonly IWebSocketManager _socket;
        public ArcaImpl(SystemVariables sysvar, IHttpClient requests, ICacheService cache = null) {
            this._sysVar = sysvar;
            this._config = _sysVar.ArcaConfig;
            _sessionState = new SessionManager<SessionState<ArcaSessionData>>();
            this._request = requests;
            _cache = cache;
            initializePCs();
        }

        public ArcaImpl(IOptionsMonitor<SystemVariables> sysvar, IHttpClient requests, IWebSocketManager socket, ICacheService cache = null) {
            this._sysVar = sysvar.CurrentValue;
            this._config = _sysVar.ArcaConfig;
            _sessionState = new SessionManager<SessionState<ArcaSessionData>>();
            this._request = requests;
            _cache = cache;
            _socket = socket;
            initializePCs();
        }

        public async Task<ArcaSessionData> getKey(string terminalID, Dictionary<Keys, byte[]>? keys = null, Keys keyType = Keys.TMK, byte[]? prerequisiteKey = null, Keys nextKey = Keys.TSK) {
            long toDayEnd = 86400 - Utilities.getTodayDate().unixTimestamp % 86400;
            if (_sessionState.exists(terminalID)) {
                var sessionObj = _sessionState.get(terminalID);
                if (!sessionObj.expired()) {
                    return sessionObj.state;
                }
            }
            if (keys == null)
                keys = new Dictionary<Keys, byte[]>();
            if (keys.Count == 3) {
                var paramObj = await parameterDownload(terminalID);
                var sessionData = new ArcaSessionData {
                    keys = keys,
                    terminalParameters = paramObj
                };
                SessionState<ArcaSessionData> _state = new SessionState<ArcaSessionData>(toDayEnd) {
                    state = sessionData
                };
                _sessionState.add(terminalID, _state);
                return sessionData;
            }
            var dateObj = getTransactionDate();
            var _clientService = new TCPConnector(_config.ip, _config.port, _config.readTimeout = 120000, _config.writeTimeout = 120000);
            await publishToSocket($"Sending Key Exchange Request of type {keyType.ToString()} to {_config.ip} Port {_config.port}");
            var _iso8583 = new Iso8583();
            string MTI = "0800";
            var kreq = new KeyExchangeRequest {
                processingCode = this.processingCodes[keyType],
                transmissionDateAndTime = dateObj.transmDate,
                localDate = dateObj.day,
                localTime = dateObj.time,
                STAN = dateObj.time,
                terminalID = terminalID,
                MTI = MTI
            };
            var smplReqBytes = _iso8583.Build<KeyExchangeRequest>(kreq, MTI);
            var response = await _clientService.sendRequest(smplReqBytes);
            var smplResponse = _iso8583.Parse<KeyExchangeResponse>(response);
            string keyUsed = "Key Derived From RSA";
            if(keyType == Keys.TMK) {
                var clearTMK = await getClearTMKey(smplResponse.controlInformation);
                keys[keyType] = clearTMK;
            } else {
                byte[] encBytes = Utilities.stringToByteArray(smplResponse.controlInformation);
                var dck = TripleDES.decrypt(encBytes, prerequisiteKey);
                keys[keyType] = dck;
                keyUsed = BitConverter.ToString(prerequisiteKey).Replace("-", "");
            }
            string message = $"Key Downloaded for {keyType.ToString()}";
            Debug.WriteLine(message);
            await publishToSocket(message);

            message = $"Decrypted {smplResponse.controlInformation} with {keyUsed}";
            Debug.WriteLine(message);
            await publishToSocket(message);

            message = "Clear PIN is " + BitConverter.ToString(keys[keyType]).Replace("-", "");
            Debug.WriteLine(message);
            await publishToSocket(message);

            message = "============================================";
            Debug.WriteLine(message);
            await publishToSocket(message);
            return await getKey(terminalID, keys, nextKey, keys[Keys.TMK], Keys.TPK);
        }

        public async Task<Parameters> parameterDownload2(string terminalID) {
            return _config.terminalParameter;
        }

        public async Task<Parameters> parameterDownload(string terminalID) {
            await publishToSocket($"Sending Parameter Download Request to {_config.ip} Port {_config.port}");
            var _clientService = new TCPConnector(_config.ip, _config.port, _config.readTimeout = 120000, _config.writeTimeout = 120000);  
            var dateObj = getTransactionDate();
            var _iso8583 = new Iso8583();
            string MTI = "0800";
            var kreq = new ParameterDownloadRequestArca {
                processingCode = "9C0000",
                transmissionDateAndTime = dateObj.transmDate,
                localDate = dateObj.day,
                localTime = dateObj.time,
                STAN = dateObj.time,
                terminalID = terminalID,
                MTI = MTI,
                privateField = $"01008{terminalID}",
                primaryMessageHashValue = Cryptography.CharGenerator.genID(64, Cryptography.CharGenerator.characterSet.HEX_STRING)
            };
            var smplReqBytes = _iso8583.Build<ParameterDownloadRequestArca>(kreq, MTI);
            Debug.WriteLine(kreq.GetDumpedMessage());
            await publishToSocket(kreq.GetDumpedMessage());
            var mf = new MessageFactory<IsoMessage> {
                Encoding = Encoding.UTF8
            };
            var isoMessage = mf.NewMessage(0x800);
            isoMessage.SetField(3, new IsoValue(IsoType.NUMERIC, "9C0000", 6));
            isoMessage.SetField(7, new IsoValue(IsoType.NUMERIC, dateObj.transmDate, 10));
            isoMessage.SetField(11, new IsoValue(IsoType.NUMERIC, dateObj.time, 6));
            isoMessage.SetField(12, new IsoValue(IsoType.NUMERIC, dateObj.time, 6));
            isoMessage.SetField(13, new IsoValue(IsoType.NUMERIC, dateObj.day, 4));
            isoMessage.SetField(41, new IsoValue(IsoType.ALPHA, terminalID, 8));
            isoMessage.SetField(62, new IsoValue(IsoType.LLLVAR, $"01008{terminalID}"));
            isoMessage.SetField(64, new IsoValue(IsoType.NUMERIC, Cryptography.CharGenerator.genID(64, Cryptography.CharGenerator.characterSet.HEX_STRING), 64));
            var buf = (byte[])(Array)isoMessage.WriteData();
            var response = await _clientService.sendRequest(buf);
            var smplResponse = _iso8583.Parse<ParameterDownloadResponse>(response);
            Debug.WriteLine(smplResponse.GetDumpedMessage());
            await publishToSocket(kreq.GetDumpedMessage());
            return smplResponse.getParameters();
        }

        public async Task<TransferResponse> createPayment(EMVStandardPayload payload) {
            var sessionConf = await getKey(payload.terminalID);
            var dateObj = Utilities.getTransactionDate();
            string pCode = $"00{((int)payload.accountType).ToString().PadLeft(2, '0')}00";
            var mf = new MessageFactory<IsoMessage> {
                Encoding = Encoding.UTF8
            };
            mf.SetConfigPath(@"\PurchaseRequestDefinition.xml");
            var isoMessage = mf.NewMessage(0x200);
            isoMessage.SetField(2, new IsoValue(IsoType.LLVAR, payload.CardData.pan));
            isoMessage.SetField(3, new IsoValue(IsoType.NUMERIC, pCode, 6));
            isoMessage.SetField(4, new IsoValue(IsoType.NUMERIC, payload.amount.ToString().PadLeft(12, '0'), 12));
            isoMessage.SetField(7, new IsoValue(IsoType.DATE10, dateObj.transmDate));
            isoMessage.SetField(11, new IsoValue(IsoType.NUMERIC, dateObj.time, 6));
            isoMessage.SetField(12, new IsoValue(IsoType.NUMERIC, dateObj.time, 6));
            isoMessage.SetField(13, new IsoValue(IsoType.NUMERIC, dateObj.day, 4));
            isoMessage.SetField(14, new IsoValue(IsoType.NUMERIC, payload.CardData.expiry, 4));
            isoMessage.SetField(18, new IsoValue(IsoType.ALPHA, sessionConf.terminalParameters.MerchantCategoryCode, 4));
            isoMessage.SetField(22, new IsoValue(IsoType.ALPHA, "051", 3));
            isoMessage.SetField(23, new IsoValue(IsoType.ALPHA, payload.cardSequenceNumber.PadLeft(3, '0'), 3));
            isoMessage.SetField(25, new IsoValue(IsoType.ALPHA, _config.posCondition, 2));
            isoMessage.SetField(26, new IsoValue(IsoType.NUMERIC, _config.posPinCaptureCode, 2));
            isoMessage.SetField(28, new IsoValue(IsoType.ALPHA, "C00000000", 9));
            isoMessage.SetField(32, new IsoValue(IsoType.LLVAR, _config.acquiringInstCode));
            isoMessage.SetField(35, new IsoValue(IsoType.LLVAR, payload.CardData.track2));
            isoMessage.SetField(37, new IsoValue(IsoType.ALPHA, payload.rrn, 12));
            isoMessage.SetField(40, new IsoValue(IsoType.NUMERIC, "221", 3));
            isoMessage.SetField(41, new IsoValue(IsoType.ALPHA, payload.terminalID, 8));
            isoMessage.SetField(42, new IsoValue(IsoType.ALPHA, sessionConf.terminalParameters.CAICode, 15));
            isoMessage.SetField(43, new IsoValue(IsoType.ALPHA, sessionConf.terminalParameters.merchantNameAndLocation, 40));
            isoMessage.SetField(49, new IsoValue(IsoType.ALPHA, sessionConf.terminalParameters.currencyCode, 3));
            if (!string.IsNullOrEmpty(payload.CardData.pinBlock)) {
                isoMessage.SetField(52, new IsoValue(IsoType.ALPHA, getEncKey(payload.CardData.pinBlock, sessionConf.keys), 16));
            } else {
                isoMessage.RemoveFields(52);
            }
            isoMessage.SetField(55, new IsoValue(IsoType.LLLVAR, Utilities.reconstructICC(payload.iccData)));
            isoMessage.SetField(123, new IsoValue(IsoType.LLLVAR, _config.posDataCode));
            isoMessage.SetField(128, new IsoValue(IsoType.ALPHA, Cryptography.CharGenerator.genID(64, Cryptography.CharGenerator.characterSet.HEX_STRING), 64));
            var buf = (byte[])(Array)isoMessage.WriteData();
            var _clientService = new TCPConnector(_config.ip, _config.port, _config.readTimeout = 120000, _config.writeTimeout = 120000);
            var smplReqBytes = buf;
            await dumpISO(isoMessage);
            var response = await _clientService.sendRequest(smplReqBytes);
            if (response == null) {
                await publishToSocket("Response was null/empty");
                throw new ServiceError("Service response was empty.");
            }
            await publishToSocket(BitConverter.ToString(response).Replace("-", ""));
            var m2d = mf.ParseMessage((sbyte[])(Array)response,
                0);
            await dumpISO(m2d);
            var raw = $"Request>\n{isoMessage.DebugString()}\nResponse>\n{m2d.DebugString()}";
            if (_cache != null)
                await _cache.addWithKey(payload.rrn, raw, 10);
            return new TransferResponse {
                Field39 = (string)m2d.GetField(39)?.Value, terminalID = payload.terminalID, AuthId = (string)m2d.GetField(38)?.Value, ReferenceNumber = payload.rrn, Stan = payload.stan,
                merchantID = sessionConf.terminalParameters?.MerchantCategoryCode,
                cardExpiry = payload.CardData.expiry,
                makedPAN = payload.CardData.pan[^4..].PadLeft(12, '*'),
                MerchantName = sessionConf.terminalParameters?.merchantNameAndLocation,
                MerchantAddress = sessionConf.terminalParameters?.merchantNameAndLocation,
                ProcessingCode = pCode,
                TransactionChannelName = "ARCA"    ,
                amount = payload.amount
            };
        }

        private async Task dumpISO(IsoMessage message) {
            for(int i= 0; i < 128; i++) {
                if (message.HasField(i)) {
                    string msg = "Field " + i.ToString().PadLeft(3, '0') + ": " + (object)message.GetField(i).Value;
                    Debug.WriteLine(msg);
                    await publishToSocket(msg);
                }
            }
        }

        private void initializePCs() {
            processingCodes = new Dictionary<Keys, string> {
                { Keys.TMK, "9A0000" },
                { Keys.TSK, "9B0000" },
                { Keys.TPK, "9G0000" }
            };
        }
        private async Task<Byte[]> getClearTMKey(string tmk) {
            await publishToSocket($"Sending TMK Request to {_config.tmkDecryptEndpoint}");
            RSA rsaObject = RSA.Create();
            rsaObject.KeySize = 2048;
            object payload = new {
                key = tmk,
                keyCryptographicType = "TRIPLE_DES",
                keyUsageType = "TMK",
                keyZone = "POS_NIBSS",
                parentKey = Convert.ToBase64String(rsaObject.ExportSubjectPublicKeyInfo()),
                parentKeyCryptographicType = "RSA",
                parentKeyUsageType = "ZMK"
            };
            Dictionary<string, string> headers = new Dictionary<string, string> {
                { "key", _config.tmkDecryptionAPIKey }
            };            
            var reqstr = JObject.FromObject(payload).ToString();
            var content = new StringContent(reqstr, Encoding.UTF8, "application/json");
            await publishToSocket($"Body Dump {reqstr}");
            await publishToSocket($"Header Dump Key:{_config.tmkDecryptionAPIKey}");
            var responseObj = await _request.sendRequest(_config.tmkDecryptEndpoint, HttpMethod.Post, content, headers);
            await publishToSocket($"Response Dump {responseObj.result}");
            if (responseObj.successful) {
                var data = JObject.Parse(responseObj.result).ToObject<KeyExResponse>();
                var dataArr = Convert.FromBase64String(data.keyUnderParent);
                var keypair = DotNetUtilities.GetRsaKeyPair(rsaObject.ExportParameters(true));
                var decryptEngine = CipherUtilities.GetCipher("RSA/NONE/NoPadding");
                var decryptEngine2 = new RsaEngine();
                decryptEngine.Init(false, keypair.Private);
                decryptEngine2.Init(false, keypair.Private);
                var decryptedKey2 = decryptEngine2.ProcessBlock(dataArr, 0, dataArr.Length);
                var decryptedKey = decryptEngine.DoFinal(dataArr);
                return decryptedKey[0..16];
            }
            throw new ServiceError($"The service responded with an invalid status {responseObj.httpStatus}");
        }
        private string getEncKey(string key, Dictionary<Keys, byte[]> keys) {
            byte[] keyRaw = Utilities.stringToByteArray(key);
            var stringEnc = TripleDES.encrypt(keyRaw, keys[Keys.TPK]);
            Debug.WriteLine($"Encrypting {key} with {BitConverter.ToString(keys[Keys.TPK]).Replace("-", "")} Yield {BitConverter.ToString(stringEnc).Replace("-", "")}");
            var stringDec = TripleDES.decrypt(stringEnc, keys[Keys.TPK]);
            Debug.WriteLine($"Reversed enc Yields {BitConverter.ToString(stringDec)}");
            return BitConverter.ToString(stringEnc).Replace("-", "");
        }

        private (string day, string time, string transmDate) getTransactionDate() {
            string m = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string d = DateTime.Now.Day.ToString().PadLeft(2, '0');
            int hour = DateTime.Now.Hour > 12 ? DateTime.Now.Hour - 12 : DateTime.Now.Hour < 1 ? 12 : DateTime.Now.Hour;
            string h = hour.ToString().PadLeft(2, '0');
            string mm = DateTime.Now.Minute.ToString().PadLeft(2, '0');
            string s = DateTime.Now.Second.ToString().PadLeft(2, '0');
            string day = m + d;
            string time = h + mm + s;
            string transmDate = day + time;
            return (day, time, transmDate);
        }

        private async Task<bool> publishToSocket(string data) {
            if (_socket is null)
                return false;
            await _socket.sendMessage(data);
            return true;
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

        public class KeyExResponse {
            public string keyUnderParent { get; set; }
        }
    }
}

using Core.Application.Exceptions;
using Core.Application.Interfaces.Identity;
using Core.Application.Interfaces.JobQueue;
using Core.Application.Interfaces.UseCases;
using Core.Application.UseCases;
using Core.Model.DTO.Configuration;
using Core.Model.DTO.Filter;
using Core.Model.DTO.Request;
using Core.Model.Enums;
using Core.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using WebAPI.Helpers;
using WebAPI.Model;

namespace WebAPI.Controllers {
    public class TransactionController : ControllerBase {
        private readonly ExceptionMessage _errHandler;
        private readonly SystemVariables _config;
        private readonly ITransactionUseCase _useCase;
        private readonly IJobQueue _queue;
        private readonly IIdentityManager _identity;
        public TransactionController(IOptionsMonitor<SystemVariables> config, ITransactionUseCase useCase, IIdentityManager identity) {
            this._config = config.CurrentValue;
            this._identity = identity;
            _errHandler = new ExceptionMessage(_config, _queue, _identity);
            _useCase = useCase;
        }

        [HttpPost, Route("v1/create-channel")]
        [Consumes("application/json")]
        public async Task<IActionResult> createChannel([FromBody] RequestBody<PaymentChannelDTO> payload) {
            try {
                var req = await _useCase.createChannel(payload.data, payload.secretKey);
                return new ObjectResult(req) { StatusCode = req.statusCode };
            } catch (Exception err) {
                return new ObjectResult(_errHandler.getMessage(err)) { StatusCode = _errHandler.statusCode };
            }
        }

        [HttpGet, Route("v1/get-channel")]
        [Consumes("application/json")]
        public async Task<IActionResult> getChannel() {
            try {
                var req = await _useCase.getChannel();
                return new ObjectResult(req) { StatusCode = req.statusCode };
            } catch (Exception err) {
                return new ObjectResult(_errHandler.getMessage(err)) { StatusCode = _errHandler.statusCode };
            }
        }

        [HttpGet, Route("v1/get-channel/{channel}")]
        [Consumes("application/json")]
        public async Task<IActionResult> getChannelFee(Channels channel) {
            try {
                var req = await _useCase.getChannelFee(channel);
                return new ObjectResult(req) { StatusCode = req.statusCode };
            } catch (Exception err) {
                return new ObjectResult(_errHandler.getMessage(err)) { StatusCode = _errHandler.statusCode };
            }
        }

        [HttpGet, Route("v1/get-transactions")]
        [Consumes("application/json")]
        public async Task<IActionResult> getTransactions([FromQuery] TransactionQuery query) {
            try {
                TransactionFilter filter = new TransactionFilter();
                if(query.channel != null) {
                    filter.channel = (Channels)query.channel;
                }
                if (query.fromDate != null && query.toDate != null) {
                    filter.transDate = ((long)query.fromDate, (long)query.toDate);
                }
                if (!string.IsNullOrEmpty(query.transID)) {
                    filter.transID = query.transID;
                }
                if (!string.IsNullOrEmpty(query.apiProfile)) {
                    filter.apiProfile = query.apiProfile;
                }
                if (!string.IsNullOrEmpty(query.terminalID)) {
                    filter.deviceID = query.terminalID;
                }
                if (query.id != null) {
                    filter.id = (int)query.id;
                }
                var req = await _useCase.getTransaction(filter);
                return new ObjectResult(req) { StatusCode = req.statusCode };
            } catch (Exception err) {
                return new ObjectResult(_errHandler.getMessage(err)) { StatusCode = _errHandler.statusCode };
            }
        }

        [HttpPut, Route("v1/update-channel")]
        [Consumes("application/json")]
        public async Task<IActionResult> updateChannel([FromBody] RequestBody<PaymentChannelDTO> payload) {
            try {
                var req = await _useCase.updateChannel(payload.data, payload.secretKey);
                return new ObjectResult(req) { StatusCode = req.statusCode };
            } catch (Exception err) {
                return new ObjectResult(_errHandler.getMessage(err)) { StatusCode = _errHandler.statusCode };
            }
        }


        [HttpPost, Route("v1/card-pay")]
        [Consumes("application/json")]
        public async Task<IActionResult> cardPay([FromBody] RequestBody<EMVStandardPayload> payload) {
            try {
                if(_identity.getHeaderValue("Request_Type") == "plain") {
                    var reqint = await _useCase.transact(payload.data);
                    return new ObjectResult(reqint) { StatusCode = reqint.statusCode };
                }
                if (string.IsNullOrEmpty(payload.transaction)) {
                    throw new InputError("Transaction Object was not found. Expecting `String`");
                }
                var key = _config.systemKey[0..32];
                string iv = _config.systemKey[32..64];
                Cryptography.AES aes = new Cryptography.AES();
                byte[] keyArr = Utilities.stringToByteArray(key);
                byte[] ivArr = Utilities.stringToByteArray(iv);
                aes.setKeys(keyArr, ivArr);
                string? decrypted = aes.decrypt(payload.transaction);
                if(decrypted == null)
                    throw new InputError("Transaction string was invalid. Please try again");
                var req = await _useCase.transact(JObject.Parse(decrypted).ToObject<EMVStandardPayload>());
                return new ObjectResult(req) { StatusCode = req.statusCode };
            } catch (Exception err) {
                return new ObjectResult(_errHandler.getMessage(err)) { StatusCode = _errHandler.statusCode };
            }
        }

        public class TransactionQuery {
            public int? id { get; set; } = null;
            public string transID { get; set; }
            public string apiProfile { get; set; }
            public string terminalID { get; set; }
            public Channels? channel { get; set; } = null;
            public long? fromDate { get; set; } = null;
            public long? toDate { get; set; } = null;
        }
    }
}

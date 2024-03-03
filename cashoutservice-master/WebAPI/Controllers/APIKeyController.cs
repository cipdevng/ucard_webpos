using Core.Application.Interfaces.Identity;
using Core.Application.Interfaces.JobQueue;
using Core.Application.Interfaces.UseCases;
using Core.Model.DTO.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebAPI.Helpers;
using WebAPI.Model;

namespace WebAPI.Controllers {
    public class APIKeyController : ControllerBase {
        private readonly ExceptionMessage _errHandler;
        private readonly SystemVariables _config;
        private readonly IAccountUseCase _useCase;
        private readonly IJobQueue _queue;
        private readonly IIdentityManager _identity;
        public APIKeyController(IOptionsMonitor<SystemVariables> config, IAccountUseCase useCase, IIdentityManager identity) {
            this._config = config.CurrentValue;
            this._identity = identity;
            _errHandler = new ExceptionMessage(_config, _queue, _identity);
            _useCase = useCase;
        }

        [HttpPost, Route("v1/create-key")]
        [Consumes("application/json")]
        public async Task<IActionResult> createKey([FromBody] RequestBody<LoginDTO> payload) {
            try {
                var req = await _useCase.createProfile(payload.data.name, payload.data.isAdmin, payload.secretKey);
                return new ObjectResult(req) { StatusCode = req.statusCode };
            } catch (Exception err) {
                return new ObjectResult(_errHandler.getMessage(err)) { StatusCode = _errHandler.statusCode };
            }
        }

        [HttpPut, Route("v1/suspend-key")]
        [Consumes("application/json")]
        public async Task<IActionResult> suspendKey([FromBody] RequestBody<LoginDTO> payload) {
            try {
                var req = await _useCase.suspendAccount(payload.data.publicKey, payload.secretKey);
                return new ObjectResult(req) { StatusCode = req.statusCode };
            } catch (Exception err) {
                return new ObjectResult(_errHandler.getMessage(err)) { StatusCode = _errHandler.statusCode };
            }
        }

        [HttpGet, Route("v1/get-keys")]
        [Consumes("application/json")]
        public async Task<IActionResult> getKeys() {
            try {
                var req = await _useCase.getAPIProfile();
                return new ObjectResult(req) { StatusCode = req.statusCode };
            } catch (Exception err) {
                return new ObjectResult(_errHandler.getMessage(err)) { StatusCode = _errHandler.statusCode };
            }
        }

    }
}

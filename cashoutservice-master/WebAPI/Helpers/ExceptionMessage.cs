using Core.Application.Interfaces.Identity;
using Core.Application.Interfaces.JobQueue;
using Core.Model.DTO.Configuration;
using Core.Model.DTO.Internal;
using Core.Model.DTO.Response;
using Org.BouncyCastle.Ocsp;
using System.Diagnostics;
using System.Security.Principal;

namespace WebAPI.Helpers {
    public class ExceptionMessage {
        private readonly SystemVariables _var;
        private readonly IJobQueue _queue;
        private readonly string routeName;
        private readonly IIdentityManager _identity;
        public int statusCode { get; private set; } = 200;
        WebResponse<object> response = new WebResponse<object>();
        public ExceptionMessage(SystemVariables _var, IJobQueue queue, IIdentityManager identity) {
            this._var = _var;
            this._queue = queue;
            this._identity = identity;
            if (!(identity is null))
                this.routeName = identity?.endPointAddress;
        }
        public WebResponse<object> getMessage(Exception _exc) {
            if (_exc is Core.Application.Exceptions.AuthenticationError) {
                var resp = response.fail(ResponseCodes.ACCESS_DENIED_ERROR, _exc.Message);
                this.statusCode = resp.statusCode;
                return resp;
            }
            if (_exc is Core.Application.Exceptions.InputError) {
                var resp = response.fail(ResponseCodes.INPUT_ERROR, _exc.Message);
                this.statusCode = resp.statusCode;
                return resp;
            }
            if(_exc is Core.Application.Exceptions.LogicError) {
                var resp = response.fail(ResponseCodes.LOGIC_ERROR, _exc.Message);
                this.statusCode = resp.statusCode;
                return resp;
            }
            if (_exc is Core.Application.Exceptions.ServiceError) {
                var resp = response.fail(ResponseCodes.SERVICE_ERROR, _exc.Message);
                this.statusCode = resp.statusCode;
                return resp;
            }
            if (_identity.getHeaderValue("debug") == "one") {
                WebResponse<object> resp;
                resp = response.fail(ResponseCodes.SYSTEM_ERROR, _exc.ToString());
                this.statusCode = resp.statusCode;
                return resp;
            }
            if (!(_queue is null)) {
                StackTrace stackTrace = new StackTrace();
                string route = stackTrace.GetFrame(1).GetMethod().Name;
                var config = _queue.configure(_var.QueueServer.Jobs.Errlog, true, null);
                _queue.send(new ErrLogDTO { details = _exc.ToString(), request = routeName, route = route, username = "" }, config);
            }
            var defaultResp = response.fail(ResponseCodes.SYSTEM_ERROR, "Service is unavailable. Try again after some time or contact admin");
            this.statusCode = defaultResp.statusCode;
            return defaultResp;
        }
    }
}

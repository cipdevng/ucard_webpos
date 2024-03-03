using Core.Application.Interfaces.Identity;
using Core.Model.DTO.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NetCore.AutoRegisterDi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Libraries.Authentication {
    [RegisterAsScoped]
    public class IdentityManager : IIdentityManager {
        private readonly SystemVariables _config;
        private readonly IHttpContextAccessor _contextAccessor;
        private protected IHeaderDictionary headers;
        private protected JObject profile;
        public bool valid { get; private set; }
        public string message { get; private set; }
        public string IPAddress { get; private set; }
        public string useragent { get; private set; }
        private readonly JWTIdentity tokenMngr;
        public string endPointAddress { get; private set; }
        public IdentityManager(IOptionsMonitor<SystemVariables> config, IHttpContextAccessor contextAccessor) {
            this._config = config.CurrentValue;
            this._contextAccessor = contextAccessor;
            this.tokenMngr = new JWTIdentity(_config.JWTSettings.jwtSecret);
            loadProfile();
            getIPAddress();
            getRoute();
            getUserAgent();
        }
        private void getRoute() {
            try {
                this.endPointAddress = _contextAccessor.HttpContext.Request.Path;
            } catch { }
        }
        private void getUserAgent() {
            try {
                this.useragent = this.getHeaderValue("User-Agent").ToString();
            } catch { }            
        }
        private void getIPAddress() {
            string[] ipAddresses = new string[] { null };
            string altIPAddress = null;
            try {
                try {
                    ipAddresses = this.getHeaderValue("X-Forwarded-For").ToString().Split(':');
                } catch { }
                altIPAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            } catch { }
            var ipAddress = string.IsNullOrEmpty(ipAddresses[0]) ? altIPAddress : ipAddresses[0];
            this.IPAddress = ipAddress;
        }
        public void loadCustomHeaders(IDictionary<string, object> header) {
            var headersUsable = new Dictionary<String, StringValues>();
            foreach (var kvp in header) {
                try {
                    try {
                        headersUsable.Add(kvp.Key, (StringValues)Encoding.Default.GetString((byte[])kvp.Value));
                    } catch {
                        var vstr = new StringValues((string)kvp.Value);
                        headersUsable.Add(kvp.Key, vstr);
                    }
                } catch {
                    throw;
                }
            }
            var headers = new HeaderDictionary(headersUsable) as IHeaderDictionary;
            this.headers = headers;
            loadProfile();
        }
        public bool sessionValid() {
            loadProfile(true);
            return valid;
        }

        private void loadProfile(bool enforceExpiryCheck = false) {
            try {
                this.headers = this.headers ?? _contextAccessor.HttpContext.Request.Headers;
                string h = headers["Authorization"];
                if (!string.IsNullOrEmpty(h)) {
                    string[] auths = h.Trim().Split(" ");
                    if (auths.Length >= 2) {
                        h = auths[^1];
                        profile = tokenMngr.verifyToken(h, enforceExpiryCheck ? enforceExpiryCheck : _config.JWTSettings.identityExpires);
                        valid = profile != null;
                        if (!valid)
                            message = "The authentication is invalid or expired";
                    } else { valid = false; this.message = "Authorization Header is not valid. Login again or contact admin"; }
                } else { valid = false; this.message = "Authorization Header is missing in request"; }
            } catch {
                valid = false;
                this.message = "Authorization Header is missing in request";
            }
        }
        public string getJWTIdentity(Dictionary<string, string> identity, int expiry = 0) {
            return tokenMngr.getToken(identity, expiry < 1 ? _config.JWTSettings.identityExpiryMins : expiry);
        }
        public string getHeaderValue(string key) {
            StringValues p;
            this.headers.TryGetValue(key, out p);
            return p;
        }
        public IDictionary<string, object> getAllHeader() {
            var comparer = StringComparer.OrdinalIgnoreCase;
            IDictionary<string, object> snew = new Dictionary<string, object>(comparer);
            foreach (KeyValuePair<string, StringValues> val in this.headers) {
                snew.Add(val.Key, val.Value.ToString());
            }
            return snew;
        }
        public T getProfile<T>() {
            return profile.ToObject<T>();
        }
    }
}

using Core.Shared;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Infrastructure.Libraries.Authentication {
    public class JWTIdentity {
        private readonly string jwtSecret;
        public JWTIdentity(string _jwtsecret) {
            jwtSecret = _jwtsecret;
        }
        public string getToken(Dictionary<string, string> fields, int minuteExpiry = 1440) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            List<Claim> claims = new List<Claim>();
            foreach (KeyValuePair<string, string> kvp in fields) {
                string val = kvp.Value;
                if (string.IsNullOrEmpty(kvp.Value)) {
                    val = "";
                }
                claims.Add(new Claim(kvp.Key, val));
            }
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims.ToArray()),
                Expires = DateTime.UtcNow.AddMinutes(minuteExpiry),
                IssuedAt = DateTime.UtcNow,
                Issuer = "",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public JObject verifyToken(string token, bool identityExpires = true) {
            try {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = validReqs(identityExpires);
                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                var t = loadProfile(token);
                return t;
            } catch {
                return null;
            }
        }

        private TokenValidationParameters validReqs(bool identityExpires) {
            return new TokenValidationParameters() {
                ValidateLifetime = identityExpires,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidIssuer = "",
                ValidAudience = "",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        }

        public JObject loadProfile(string h) {
            string lki = Utilities.validBase64(h.Split(".")[1]);
            byte[] hpayload = Convert.FromBase64String(lki);
            JObject obj = JObject.Parse(Encoding.UTF8.GetString(hpayload));
            return obj;
        }
    }
}

using Infrastructure.Abstraction.HTTP;
using Infrastructure.DTO;
using NetCore.AutoRegisterDi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Libraries.HTTP {
    [RegisterAsSingleton]
    public class HttpWebRequestClient : IHttpClient {
        private HttpClient _client;
        public HttpWebRequestClient() {
            _client = new HttpClient();
        }
        public async Task<HttpClientResponseMessage> sendRequest(string url, HttpMethod method, HttpContent? content, Dictionary<string, string> headers) {
            HttpClientResponseMessage response = new HttpClientResponseMessage();
            try {
                var res = await this.requestHandler(url, method, content, headers);
                response.httpStatus = (int)res.StatusCode;
                response.successful = false;
                if (res.IsSuccessStatusCode) {
                    response.successful = true;
                    response.result = await res.Content.ReadAsStringAsync();
                } else {
                    response.error = await res.Content.ReadAsStringAsync();
                }
            } catch (Exception err) {
                response.successful = false;
                response.httpStatus = -1;
                response.exception = err;
            }
            return response;
        }

        private async Task<HttpResponseMessage> requestHandler(string url, HttpMethod method, HttpContent content, Dictionary<string, string> headers) {
            using (var client = new HttpClient()) {
                var req = new HttpRequestMessage(method, url);
                if (content != null) {
                    req.Content = content;
                }
                if (headers != null) {
                    foreach (KeyValuePair<string, string> header in headers) {
                        req.Headers.Add(header.Key, header.Value);
                    }
                }
                var res = await client.SendAsync(req);
                return res;
            }
        }
    }
}

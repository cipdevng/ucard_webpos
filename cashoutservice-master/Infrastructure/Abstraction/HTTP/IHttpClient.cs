using Infrastructure.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Abstraction.HTTP {
    public interface IHttpClient {
        Task<HttpClientResponseMessage> sendRequest(string url, HttpMethod method, HttpContent? content, Dictionary<string, string> headers);
    }
}

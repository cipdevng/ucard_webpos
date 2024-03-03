using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.Interfaces.Identity {
    public interface IIdentityManager {
        string message { get; }
        string useragent { get; }
        bool valid { get; }
        bool sessionValid();
        string getJWTIdentity(Dictionary<string, string> identity, int expiry = 0);
        T getProfile<T>();
        string getHeaderValue(string key);
        IDictionary<string, object> getAllHeader();
        void loadCustomHeaders(IDictionary<string, object> header);
        string IPAddress { get; }
        string endPointAddress { get; }
    }
}

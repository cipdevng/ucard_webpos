using Core.Model.DTO.Configuration;
using Core.Shared;
using Infrastructure.DTO.Enums;
using Infrastructure.Libraries.Banks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO {
    public class SessionManager<T> where T : SessionBase {
        private ConcurrentDictionary<string, T> _sessions;
        public SessionManager() {
            _sessions = new ConcurrentDictionary<string, T>();
        }

        public bool exists(string k) {
            return _sessions.ContainsKey(k);
        }

        public T get(string k) {
            if (exists(k))
                return _sessions[k];
            throw new NullReferenceException();
        }

        public void add(string k, T data) {
            _sessions.AddOrUpdate(k, (k) => { return data; }, (k, v) => { return data; });
        }

        public void remove(string k) {
            T data;
            _sessions.Remove(k, out data);
        }
    }

    public class SessionBase {
        public long expiry { get; private set; }
        public long dateAdded { get; private set; }
        public SessionBase(long expiryInSecs) {
            this.expiry = Utilities.getTodayDate().unixTimestamp + expiryInSecs;
            dateAdded = Utilities.getTodayDate().unixTimestamp;
        }
        public bool expired() {
            return expiry < Utilities.getTodayDate().unixTimestamp;
        }
    }

    public class SessionState<T> : SessionBase {
        public T state { get; set; }
        public SessionState(long expiry) : base(expiry) {

        }
    }

    public class ArcaSessionData {
        public Dictionary<Keys, byte[]> keys;
        public Parameters terminalParameters;
    }
    
    public class UPSessionData {
        public Dictionary<Keys, byte[]> keys;
        public Parameters terminalParameters;
    }    
}

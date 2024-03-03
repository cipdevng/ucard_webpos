using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Abstraction.Database {
    public interface IDBCommand {
        int lastAffectedRows { get; }
        void prepare(string sql, bool isAt = true);
        void bindValue(string key, object value);
        void bindValues(IEnumerable<object> values);
        bool clearParams();
        Task beginTransaction();
        Task<bool> execute();
        Task<IEnumerable<Dictionary<string, object>>> fetchAllAsObj();
        Task<string> fetchAllAsStr();
        public Task<bool> commit();
        void close();
    }
}

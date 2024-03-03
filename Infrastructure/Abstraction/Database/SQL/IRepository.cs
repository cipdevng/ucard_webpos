using Infrastructure.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Abstraction.Database.SQL {
    public interface IRepository {
        Task beginTransacion();
        Task<bool> commit();
        Task<bool> isExist(string sql, IEnumerable<object> q);
        Task<QueryResult<T>> selectFromQuery<T>(string sql, IEnumerable<object> q);
    }
}

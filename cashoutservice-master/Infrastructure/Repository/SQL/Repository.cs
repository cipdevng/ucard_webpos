using AutoMapper;
using Infrastructure.Abstraction.Database;
using Infrastructure.Abstraction.Database.SQL;
using Infrastructure.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.SQL {
    public class Repository : IRepository {
        private IDBCommand IDBCommand;
        private readonly IMapper _mapper;
        public Repository(IDBCommand IDBCommand, IMapper _mapper) {
            this.IDBCommand = IDBCommand;
            this._mapper = _mapper;
        }

        public async Task beginTransacion() {
            await IDBCommand.beginTransaction();
        }

        public async Task<bool> commit() {
            return await IDBCommand.commit();
        }

        public async Task<bool> isExist(string sql, IEnumerable<object> q) {
            try {
                var result = (List<Dictionary<string, object>>)(await selectFromQuery(sql, (List<object>)q));
                if (result != null && result.Count > 0) {
                    return true;
                }
                return false;
            } catch  {
                throw;
            }
        }
        public async Task<QueryResult<T>> selectFromQuery<T>(string sql, IEnumerable<object> q) {
            var res = new QueryResult<T>();
            var data = await this.selectFromQuery(sql, (List<object>)q);
            res.resultAsObject = _mapper.Map<List<T>>(data);
            return res;
        }

        private async Task<IEnumerable<Dictionary<string, object>>> selectFromQuery(string sql, List<object> q) {
            IDBCommand.prepare(sql);
            if (q != null) { IDBCommand.bindValues(q); }
            var data = await IDBCommand.fetchAllAsObj();
            return data;
        }        
    }
}

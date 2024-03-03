using AutoMapper;
using Core.Application.Interfaces.Repository.MySQL;
using Core.Model.DTO.Filter;
using Core.Model.Entities;
using Core.Model.Enums;
using Infrastructure.Abstraction.Database;
using Infrastructure.Abstraction.Database.SQL;
using Nest;
using NetCore.AutoRegisterDi;

namespace Infrastructure.Repository.SQL {
    [RegisterAsScoped]
    public class FeesRepository : Repository, IRepository, IFeesRepository {
        private readonly IDBCommand _DBcontext;
        private readonly IMapper _mapper;
        public FeesRepository(IDBCommand IDBCommand, IMapper _mapper) : base(IDBCommand, _mapper) {
            _DBcontext = IDBCommand;
            this._mapper = _mapper;
        }

        public async Task<bool> create(Fees fee) {
            string query = "INSERT INTO fees (upperTransactionBound, lowerTransactionBound, chargeValue, chargeIsFlat, cap, channel) VALUES (@upperTransactionBound, @lowerTransactionBound, @chargeValue, @chargeIsFlat, @cap, @channel)";
            _DBcontext.prepare(query);
            _DBcontext.bindValue("@upperTransactionBound", fee.upperTransactionBound);
            _DBcontext.bindValue("@lowerTransactionBound", fee.lowerTransactionBound);
            _DBcontext.bindValue("@chargeValue", fee.chargeValue);
            _DBcontext.bindValue("@chargeIsFlat", fee.chargeIsFlat);
            _DBcontext.bindValue("@cap", fee.cap);
            _DBcontext.bindValue("@channel", (int)fee.channel);
            return await _DBcontext.execute();
        }

        public async Task<bool> create(List<Fees> fees) {
            await _DBcontext.beginTransaction();
            if (fees.Count < 1)
                return false;
            await this.delete(fees[0].channel);
            foreach(Fees fee in fees) {
                string query = "INSERT INTO fees (upperTransactionBound, lowerTransactionBound, chargeValue, chargeIsFlat, cap, channel) VALUES (@upperTransactionBound, @lowerTransactionBound, @chargeValue, @chargeIsFlat, @cap, @channel)";
                _DBcontext.prepare(query);
                _DBcontext.bindValue("@upperTransactionBound", fee.upperTransactionBound);
                _DBcontext.bindValue("@lowerTransactionBound", fee.lowerTransactionBound);
                _DBcontext.bindValue("@chargeValue", fee.chargeValue);
                _DBcontext.bindValue("@chargeIsFlat", fee.chargeIsFlat);
                _DBcontext.bindValue("@cap", fee.cap);
                _DBcontext.bindValue("@channel", (int)fee.channel);
                return await _DBcontext.execute();
            }
            return await _DBcontext.commit();
        }

        public async Task<bool> delete(Channels channel) {
            string query = "DELETE FROM fees WHERE channel = @channel";
            _DBcontext.prepare(query);
            _DBcontext.bindValue("@channel", channel);
            return await _DBcontext.execute();
        }

        public async Task<List<Fees>> get(Channels channel) {
            var result = (List<Fees>)(await selectFromQuery<Fees>("SELECT * FROM fees WHERE channel = ?", new List<object> { (int)channel })).resultAsObject;
            return result;
        }
    }
}

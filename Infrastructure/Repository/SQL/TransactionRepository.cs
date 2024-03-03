using AutoMapper;
using Core.Application.Interfaces.Repository.MySQL;
using Core.Model.DTO.Filter;
using Core.Model.Entities;
using Infrastructure.Abstraction.Database;
using Infrastructure.Abstraction.Database.SQL;
using Nest;
using NetCore.AutoRegisterDi;

namespace Infrastructure.Repository.SQL {
    [RegisterAsScoped]
    public class TransactionRepository : Repository, IRepository, ITransactionRepository {
        private readonly IDBCommand _DBcontext;
        private readonly IMapper _mapper;
        private readonly IPaymentChannelRepository _paymentchannelRepo;
        public TransactionRepository(IDBCommand IDBCommand, IMapper _mapper, IPaymentChannelRepository paymentchannelRepo) : base(IDBCommand, _mapper) {
            this._DBcontext = IDBCommand;
            this._mapper = _mapper;
            this._paymentchannelRepo = paymentchannelRepo;
        }
        public async Task<bool> create(Transaction transaction) {
            string query = "INSERT INTO transaction (requestPayload, transID, transDate, deviceID, response, rawResponse, apiProfile, channel, status) VALUES (@requestPayload, @transID, @transDate, @deviceID, @response, @rawResponse, @apiProfile, @channel, @status)";
            _DBcontext.prepare(query);
            _DBcontext.bindValue("@requestPayload", transaction.requestPayload);
            _DBcontext.bindValue("@transID", transaction.transID);
            _DBcontext.bindValue("@transDate", transaction.transDate);
            _DBcontext.bindValue("@deviceID", transaction.deviceID);
            _DBcontext.bindValue("@response", transaction.response);
            _DBcontext.bindValue("@rawResponse", transaction.rawResponse);
            _DBcontext.bindValue("@apiProfile", transaction.apiProfile);
            _DBcontext.bindValue("@channel", (int)transaction.channel);
            _DBcontext.bindValue("@status", transaction.status);
            return await _DBcontext.execute();
        }

        public async Task<List<Transaction>> get(TransactionFilter filter) {
            string clause = string.Empty;
            List<object> param = new List<object>();
            if (filter.fieldIsSet(nameof(filter.id))) {
                clause += " AND id = ?";
                param.Add(filter.id);
            }
            if (filter.fieldIsSet(nameof(filter.transID))) {
                clause += " AND transID = ?";
                param.Add(filter.transID);
            }
            if (filter.fieldIsSet(nameof(filter.apiProfile))) {
                clause += " AND apiProfile = ?";
                param.Add(filter.apiProfile);
            }
            if (filter.fieldIsSet(nameof(filter.deviceID))) {
                clause += " AND deviceID = ?";
                param.Add(filter.deviceID);
            }
            if (filter.fieldIsSet(nameof(filter.transID))) {
                clause += " AND transID = ?";
                param.Add(filter.transID);
            }
            if (filter.fieldIsSet(nameof(filter.transDate))) {
                clause += " AND transDate BETWEEN ? AND ?";
                param.Add(filter.transDate.fromDate);
                param.Add(filter.transDate.toDate);
            }
            if (filter.fieldIsSet(nameof(filter.channel))) {
                clause += " AND channel = ?";
                param.Add(filter.channel);
            }

            var result = (List<Transaction>)(await selectFromQuery<Transaction>("SELECT * from transaction WHERE id > 0 " + clause + " ORDER BY transDate DESC", param)).resultAsObject;
            return result;
        }

        public async Task<bool> updateResponse(Transaction transaction) {
            string query = "UPDATE transaction set response = @response, rawResponse = @rawResponse, status = @status WHERE transID = @transID";
            _DBcontext.prepare(query);
            _DBcontext.bindValue("@response", transaction.response);
            _DBcontext.bindValue("@transID", transaction.transID);
            _DBcontext.bindValue("@rawResponse", transaction.rawResponse);
            _DBcontext.bindValue("@status", transaction.status);
            await _DBcontext.execute();
            return await _paymentchannelRepo.setStatistics((int)transaction.channel, transaction.status < 1);
        }
    }
}

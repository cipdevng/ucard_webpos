using AutoMapper;
using Core.Application.Interfaces.Repository.MySQL;
using Core.Model.DTO.Filter;
using Core.Model.DTO.Response;
using Core.Model.Entities;
using Core.Model.Enums;
using Infrastructure.Abstraction.Database;
using Infrastructure.Abstraction.Database.SQL;
using Nest;
using NetCore.AutoRegisterDi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Infrastructure.Repository.SQL
{
    [RegisterAsScoped]
    public class PaymentChannelRepository : Repository, IRepository, IPaymentChannelRepository {
        private readonly IDBCommand _DBcontext;
        private readonly IMapper _mapper;
        private readonly IFeesRepository _feeRepo;
        public PaymentChannelRepository(IDBCommand IDBCommand, IMapper _mapper, IFeesRepository feeRepo) : base(IDBCommand, _mapper)
        {
            _DBcontext = IDBCommand;
            this._mapper = _mapper;
            this._feeRepo = feeRepo;
        }

        public async Task<bool> create(PaymentChannels channel, List<Fees> fees) {
            await _DBcontext.beginTransaction();
            string query = "INSERT INTO paymentchannel (channel, status, priority, transactionCount, transactionCountFailure, dateCreated) VALUES (@channel, @status, @priority, @transactionCount, @transactionCountFailure, @dateCreated)";
            _DBcontext.prepare(query);
            _DBcontext.bindValue("@channel", (int)channel.channel);
            _DBcontext.bindValue("@status", (int)channel.status);
            _DBcontext.bindValue("@priority", channel.priority);
            _DBcontext.bindValue("@transactionCount", channel.transactionCount);
            _DBcontext.bindValue("@transactionCountFailure", channel.transactionCountFailure);
            _DBcontext.bindValue("@dateCreated", channel.dateCreated);
            await _DBcontext.execute();
            foreach(Fees fee in fees) {
                await _feeRepo.create(fee);
            }
            return await _DBcontext.commit();
        }

        public async Task<List<PaymentChannels>> get(PaymentChannelFilter filter) {
            string clause = string.Empty;
            List<object> param = new List<object>();
            if (filter.fieldIsSet(nameof(filter.id))) {
                clause += " AND paymentchannel.id = ?";
                param.Add(filter.id);
            }
            if (filter.fieldIsSet(nameof(filter.channel))) {
                clause += " AND paymentchannel.channel = ?";
                param.Add(filter.channel);
            }
            if (filter.fieldIsSet(nameof(filter.amount))) {
                clause += " AND ? BETWEEN fees.lowerTransactionBound AND upperTransactionBound";
                param.Add(filter.amount);
            }
            if (filter.fieldIsSet(nameof(filter.status))) {
                clause += " AND status = ?";
                param.Add(filter.status);
            }
            
            var result = (List<PaymentChannels>)(await selectFromQuery<PaymentChannels>("SELECT paymentchannel.*, fees.upperTransactionBound, fees.lowerTransactionBound, fees.chargeValue, fees.chargeIsFlat, fees.cap from paymentchannel, fees WHERE paymentchannel.channel = fees.channel " + clause + " ORDER BY priority DESC", param)).resultAsObject;
            return result;
        }

        public async Task<PaymentChannelResponse?> get(int id) {
            var result = (List<PaymentChannelResponse>)(await selectFromQuery<PaymentChannelResponse>("SELECT * FROM paymentchannel WHERE id = ?", new List<object> { id })).resultAsObject;
            var resultData = result.FirstOrDefault();
            if (resultData == null)
                return null;
            var fees = await _feeRepo.get(resultData.channel);
            resultData.fees = fees;
            return resultData;
        }

        public async Task<List<PaymentChannelBase>> get() {
            var result = (List<PaymentChannelBase>)(await selectFromQuery<PaymentChannelBase>("SELECT * FROM paymentchannel", new List<object> {  })).resultAsObject;
            return result;
        }

        public async Task<PaymentChannelResponse?> get(Channels Channel) {
            var result = (List<PaymentChannelResponse>)(await selectFromQuery<PaymentChannelResponse>("SELECT * FROM paymentchannel WHERE channel = ?", new List<object> { (int)Channel })).resultAsObject;
            var resultData = result.FirstOrDefault();
            if (resultData == null)
                return null;
            var fees = await _feeRepo.get(resultData.channel);
            resultData.fees = fees;
            return resultData;
        }

        public async Task<bool> setPriority(long id, int priority, int status) {
            string query = "UPDATE paymentchannel SET priority = @priority WHERE id = @id";
            _DBcontext.prepare(query);
            _DBcontext.bindValue("@priority", priority);
            _DBcontext.bindValue("@status", status);
            _DBcontext.bindValue("@id", id);
            return await _DBcontext.execute();
        }

        public async Task<bool> setStatistics(long id, bool requestFailed) {
            string optional = requestFailed? ", transactionCountFailure = transactionCountFailure + 1" : "";
            string query = $"UPDATE paymentchannel SET transactionCount = transactionCount + 1 {optional} WHERE channel = @id";
            _DBcontext.prepare(query);
            _DBcontext.bindValue("@id", id);
            return await _DBcontext.execute();
        }
    }
}

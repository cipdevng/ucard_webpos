using Core.Application.Exceptions;
using Core.Application.Interfaces.Bank;
using Core.Application.Interfaces.Cache;
using Core.Application.Interfaces.Identity;
using Core.Application.Interfaces.Repository.MySQL;
using Core.Application.Interfaces.UseCases;
using Core.Model.DTO.Configuration;
using Core.Model.DTO.Filter;
using Core.Model.DTO.Request;
using Core.Model.DTO.Response;
using Core.Model.Entities;
using Core.Model.Enums;
using Microsoft.Extensions.Options;
using NetCore.AutoRegisterDi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Core.Application.UseCases {
    [RegisterAsScoped]
    public class TransactionUseCase : UseCase, ITransactionUseCase {
        private readonly ITransactionRepository _trxRepo;
        private readonly IEMVContainer _emvContainer;
        private readonly IFeesRepository _feeRepo;
        private readonly IPaymentChannelRepository _paymentChannelRepo;
        private readonly ICacheService _cache;
        private readonly SystemVariables _sysvar;
        public TransactionUseCase(IIdentityManager idenity, IAPIProfileRepository profileRepo, ITransactionRepository trxRepo, IEMVContainer emvContaimer, IFeesRepository feeRepo, IPaymentChannelRepository paymentchannelRepo, IOptionsMonitor<SystemVariables> sysVar, ICacheService cache) : base(idenity, profileRepo) {
            _trxRepo = trxRepo;
            _emvContainer = emvContaimer;
            _sysvar = sysVar.CurrentValue;
            _feeRepo = feeRepo;
            _paymentChannelRepo = paymentchannelRepo;
            _cache = cache;
        }

        public async Task<WebResponse<object>> createChannel(PaymentChannelDTO request, string secretKey) {
            WebResponse response = new WebResponse();
            await loadIdentity();
            if(activeProfile.isAdmin != 1) {
                throw new AuthenticationError("Access Denied");
            }
            if(activeProfile.privateKey != secretKey)
                throw new AuthenticationError("Access Denied. Invalid secret key");
            if (request.channel == null)
                throw new InputError("Invalid Channel");
            var channelExist = await _paymentChannelRepo.get((Channels)request.channel);
            if (channelExist != null)
                return response.fail(ResponseCodes.INVALID_REQUEST, "This Channel already exists");
            PaymentChannels channel = new PaymentChannels(request);
            List<Fees> fees = new List<Fees>();
            foreach(FeesDTO dto in request.fees) {
                fees.Add(new Fees(dto, channel));
            }
            await _paymentChannelRepo.create(channel, fees);
            return response.success();
        }

        public Task<WebResponse<object>> getBalance(EMVStandardPayload payload) {
            throw new NotImplementedException();
        }

        public async Task<WebResponse<object>> getChannel() {
            WebResponse response = new WebResponse();
            await loadIdentity();
            if (activeProfile.isAdmin != 1) {
                throw new AuthenticationError("Access Denied");
            }
            var channel = await _paymentChannelRepo.get();
            return response.success(channel);
        }

        public async Task<WebResponse<object>> getChannelFee(Channels channel) {
            WebResponse response = new WebResponse();
            await loadIdentity();
            if (activeProfile.isAdmin != 1) {
                throw new AuthenticationError("Access Denied");
            }
            var channelData = await _paymentChannelRepo.get(channel);
            return response.success(channelData);
        }

        public async Task<WebResponse<object>> getTransaction(TransactionFilter filter) {
            WebResponse response = new WebResponse();
            await loadIdentity();
            var data = await _trxRepo.get(filter);
            return response.success(data);
        }

        public async Task<WebResponse<object>> transact(EMVStandardPayload payload) {
            WebResponse response = new WebResponse();
            await loadIdentity();
            PaymentChannelBase? channel = null;
            if (payload.preferredChannel != null) {
                channel = await _paymentChannelRepo.get((Channels)payload.preferredChannel);
                if (channel == null)
                    return response.fail(ResponseCodes.INVALID_REQUEST, "The channel is invalid");
                if(channel.status != ChannelStatus.ACTIVE)
                    return response.fail(ResponseCodes.INVALID_REQUEST, "The channel is currently inactive or deactivated");
            } else {
                var channels = await _paymentChannelRepo.get(new PaymentChannelFilter { amount = payload.amount/100, status = ChannelStatus.ACTIVE });
                if (channels.Count < 1)
                    return response.fail(ResponseCodes.INVALID_REQUEST, "Could not find a suitable channel");
                channel = channels.OrderBy(O => O.getFee(payload.amount / 100)).FirstOrDefault();
            }
            if(channel is null)
                return response.fail(ResponseCodes.INVALID_REQUEST, "Could not find a suitable channel");
            payload.preferredChannel = channel.channel;
            Transaction trx = new Transaction(payload, activeProfile.publicKey);
            var processor = await _emvContainer.getService((Channels)payload.preferredChannel);
            await _trxRepo.create(trx);
            if (processor is null)
                return response.fail(ResponseCodes.SYSTEM_ERROR, $"Could not find a suitable processor matching {payload.preferredChannel.ToString()}");
            try {
                var result = await processor.createPayment(payload);
                int status = -1;
                string raw = await _cache.getWithKey(trx.transID);
                if(result.Field39 == "00") {
                    status = 1;
                }
                var t = JObject.FromObject(result).ToString();
                trx.setTransactionStatus(t, raw, status);
                await _trxRepo.updateResponse(trx);
                return response.success(result);
            } catch(Exception err) {
                trx.setTransactionStatus(err.Message, err.ToString(), -1);
                await _trxRepo.updateResponse(trx);
                throw;
            }
        }

        public async Task<WebResponse<object>> updateChannel(PaymentChannelDTO request, string secretKey) {
            WebResponse response = new WebResponse();
            await loadIdentity();
            if (activeProfile.isAdmin != 1) {
                throw new AuthenticationError("Access Denied");
            }
            if (activeProfile.privateKey != secretKey)
                throw new AuthenticationError("Access Denied. Invalid secret key");
            if (request.channel == null)
                throw new InputError("Invalid Channel");
            if(request.status == null)
                throw new InputError("Invalid Status");
            var channelExist = await _paymentChannelRepo.get((Channels)request.channel);
            if (channelExist == null)
                return response.fail(ResponseCodes.INVALID_REQUEST, "This Channel does not exist");            
            if(request.fees != null && request.fees.Count > 0) {
                List<Fees> fees = new List<Fees>();
                foreach (FeesDTO dto in request.fees) {
                    fees.Add(new Fees(dto, channelExist));
                }
                await _feeRepo.create(fees);
            }
            await _paymentChannelRepo.setPriority(channelExist.id, request.priority, (int)request.status);
            return response.success();
        }
    }
}

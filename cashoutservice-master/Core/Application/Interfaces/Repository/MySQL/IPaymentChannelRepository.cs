using Core.Model.DTO.Filter;
using Core.Model.DTO.Response;
using Core.Model.Entities;
using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces.Repository.MySQL {
    public interface IPaymentChannelRepository {
        Task<bool> create(PaymentChannels channel, List<Fees> fees);
        Task<List<PaymentChannels>> get(PaymentChannelFilter filter);
        Task<bool> setStatistics(long id, bool requestFailed);
        Task<bool> setPriority(long id, int priority, int status);
        Task<PaymentChannelResponse?> get(int id);
        Task<PaymentChannelResponse?> get(Channels Channel);
        Task<List<PaymentChannelBase>> get();
    }
}

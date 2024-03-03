using Core.Model.DTO.Filter;
using Core.Model.DTO.Request;
using Core.Model.DTO.Response;
using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces.UseCases {
    public interface ITransactionUseCase {
        Task<WebResponse<object>> transact(EMVStandardPayload payload);
        Task<WebResponse<object>> getBalance(EMVStandardPayload payload);
        Task<WebResponse<object>> createChannel(PaymentChannelDTO request, string secretKey);
        Task<WebResponse<object>> updateChannel(PaymentChannelDTO request, string secretKey);
        Task<WebResponse<object>> getChannel();
        Task<WebResponse<object>> getChannelFee(Channels channel);
        Task<WebResponse<object>> getTransaction(TransactionFilter filter);
    }
}

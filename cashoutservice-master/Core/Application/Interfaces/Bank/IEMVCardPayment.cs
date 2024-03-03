using Core.Model.DTO.Request;
using Core.Model.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Model.DTO.Response.KimonoPurchaseResponse;

namespace Core.Application.Interfaces.Bank {
    public interface IEMVCardPayment {
        Task<TransferResponse> createPayment(EMVStandardPayload payload);
        Task<TransactionRequeryResponse> requery(EMVStandardPayload payload);
        Task<double> getBalance(EMVStandardPayload payload);
        Task<double> processRefund(EMVStandardPayload payload);
    }
}

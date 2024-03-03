using Core.Model.DTO.Filter;
using Core.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces.Repository.MySQL {
    public interface ITransactionRepository {
        Task<bool> create(Transaction transaction);
        Task<bool> updateResponse(Transaction transaction);
        Task<List<Transaction>> get(TransactionFilter filter);
    }
}

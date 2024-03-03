using Core.Model.DTO.Filter;
using Core.Model.Entities;
using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces.Repository.MySQL {
    public interface IFeesRepository {
        Task<bool> create(Fees fee);
        Task<bool> delete(Channels channel);
        Task<List<Fees>> get(Channels channel);
        Task<bool> create(List<Fees> fees);
    }
}

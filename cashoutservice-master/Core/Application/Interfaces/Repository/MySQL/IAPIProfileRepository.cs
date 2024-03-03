using Core.Model.DTO.Filter;
using Core.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces.Repository.MySQL {
    public interface IAPIProfileRepository {
        Task<bool> create(APIProfile profile);
        Task<bool> updateStatus(APIProfile profile);
        Task<List<APIProfile>> get(APIProfileFilter filter);
    }
}

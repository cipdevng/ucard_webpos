using Core.Model.DTO.Filter;
using Core.Model.DTO.Request;
using Core.Model.DTO.Response;
using Core.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces.UseCases {
    public interface IAccountUseCase {
        Task<WebResponse<object>> getAPIProfile();
        Task<WebResponse<object>> createProfile(string name, int isAdmin, string secretKey);
        Task<WebResponse<object>> suspendAccount(string publicKey, string secretKey);
        Task<bool> loadAuthenticatation(string apiKey, string secretKey);
    }
}

using Core.Application.Exceptions;
using Core.Application.Interfaces.Identity;
using Core.Application.Interfaces.Repository.MySQL;
using Core.Application.Interfaces.UseCases;
using Core.Model.DTO.Response;
using Core.Model.Entities;
using NetCore.AutoRegisterDi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.UseCases {
    [RegisterAsScoped]
    public class AccountUseCase : UseCase, IAccountUseCase {
        public AccountUseCase(IIdentityManager idenity, IAPIProfileRepository profileRepo) :base(idenity, profileRepo) {

        }

        public async Task<WebResponse<object>> createProfile(string name, int isAdmin, string secretKey) {
            WebResponse response = new WebResponse();
            await loadIdentity();
            if (activeProfile.isAdmin != 1)
                throw new AuthenticationError();
            if (activeProfile.privateKey != secretKey)
                throw new AuthenticationError("Access Denied. Invalid secret key");
            APIProfile profile = new APIProfile(name, isAdmin);
            await _apiProfile.create(profile);
            return response.success(profile);
        }

        public async Task<WebResponse<object>> getAPIProfile() {
            WebResponse response = new WebResponse();
            await loadIdentity();
            if (activeProfile.isAdmin != 1)
                throw new AuthenticationError();
            if (activeProfile.privateKey != _idenity.getHeaderValue("SecretKey"))
                throw new AuthenticationError("Access Denied. Invalid secret key");
            var profiles = await _apiProfile.get(new Model.DTO.Filter.APIProfileFilter { });
            return response.success(profiles);
        }

        public async Task<WebResponse<object>> suspendAccount(string publicKey, string secretKey) {
            WebResponse response = new WebResponse();
            await loadIdentity();
            if (activeProfile.isAdmin != 1)
                throw new AuthenticationError();
            if (activeProfile.privateKey != secretKey)
                throw new AuthenticationError("Access Denied. Invalid secret key");
            var profile = (await _apiProfile.get(new Model.DTO.Filter.APIProfileFilter { publicKey = publicKey })).FirstOrDefault();
            if (profile == null)
                return response.fail(ResponseCodes.FILE_NOT_FOUND);
            profile.suspend();
            await _apiProfile.updateStatus(profile);
            return response.success();
        }

        public async Task<bool> loadAuthenticatation(string apiKey, string secretKey) {
            var profile = await _apiProfile.get(new Model.DTO.Filter.APIProfileFilter { publicKey = apiKey, secretKey = secretKey });
            activeProfile = profile.FirstOrDefault();
            if (activeProfile == null)
                return false;
            return true;
        }
    }
}

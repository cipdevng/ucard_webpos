using Core.Application.Exceptions;
using Core.Application.Interfaces.Identity;
using Core.Application.Interfaces.Repository.MySQL;
using Core.Model.DTO.External;
using Core.Model.DTO.Internal;
using Core.Model.Entities;
using Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.UseCases {
    public class UseCase {
        protected readonly IIdentityManager _idenity;
        protected readonly IAPIProfileRepository _apiProfile;
        protected APIProfile activeProfile;
        public UseCase(IIdentityManager idenity, IAPIProfileRepository profileRepo) {
            _idenity = idenity;
            _apiProfile = profileRepo;
        }
        
        public async Task loadIdentity() {
            var apiKey = _idenity.getHeaderValue("API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                throw new AuthenticationError("API KEY is missing your request");
            var profile = await _apiProfile.get(new Model.DTO.Filter.APIProfileFilter { publicKey = apiKey });
            activeProfile = profile.FirstOrDefault();
            if (activeProfile == default)
                throw new AuthenticationError("Invalid API KEY");
        }
    }
}

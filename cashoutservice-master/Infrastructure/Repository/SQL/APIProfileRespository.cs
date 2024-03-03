using AutoMapper;
using Core.Application.Interfaces.Repository.MySQL;
using Core.Model.DTO.Filter;
using Core.Model.Entities;
using Infrastructure.Abstraction.Database;
using Infrastructure.Abstraction.Database.SQL;
using NetCore.AutoRegisterDi;

namespace Infrastructure.Repository.SQL {
    [RegisterAsScoped]
    public class APIProfileRespository : Repository, IRepository, IAPIProfileRepository {
        private readonly IDBCommand _DBcontext;
        private readonly IMapper _mapper;
        private readonly IFeesRepository _feeRepo;
        public APIProfileRespository(IDBCommand IDBCommand, IMapper _mapper) : base(IDBCommand, _mapper) {
            _DBcontext = IDBCommand;
            this._mapper = _mapper;
        }

        public async Task<bool> create(APIProfile profile) {
            string query = "INSERT INTO apiprofile (profileName, isAdmin, publicKey, privateKey, dateCreated, status) VALUES (@profileName, @isAdmin, @publicKey, @privateKey, @dateCreated, @status)";
            _DBcontext.prepare(query);
            _DBcontext.bindValue("@profileName", profile.profileName);
            _DBcontext.bindValue("@isAdmin", profile.isAdmin);
            _DBcontext.bindValue("@publicKey", profile.publicKey);
            _DBcontext.bindValue("@privateKey", profile.privateKey);
            _DBcontext.bindValue("@dateCreated", profile.dateCreated);
            _DBcontext.bindValue("@status", profile.status);
            return await _DBcontext.execute();
        }

        public async Task<List<APIProfile>> get(APIProfileFilter filter) {
            string clause = string.Empty;
            List<object> param = new List<object>();
            if (filter.fieldIsSet(nameof(filter.publicKey))) {
                clause += " AND publicKey = ?";
                param.Add(filter.publicKey);
            }
            if (filter.fieldIsSet(nameof(filter.secretKey))) {
                clause += " AND privateKey = ?";
                param.Add(filter.secretKey);
            }
            var result = (List<APIProfile>)(await selectFromQuery<APIProfile>("SELECT * FROM apiprofile WHERE id > 0 " + clause + " ORDER BY id ASC", param)).resultAsObject;
            return result;
        }


        public async Task<bool> updateStatus(APIProfile profile) {
            string query = "UPDATE apiprofile SET status = @status WHERE id = @id";
            _DBcontext.prepare(query);
            _DBcontext.bindValue("@id", profile.id);
            _DBcontext.bindValue("@status", profile.status);
            return await _DBcontext.execute();
        }
    }
}

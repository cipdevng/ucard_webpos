using Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Entities {
    public class APIProfile : BaseEntity {
        public string profileName { get; protected set; }
        public int isAdmin { get; protected set; }
        public string publicKey { get; protected set; }
        public string privateKey { get; protected set; }
        public long dateCreated { get; protected set; }
        public int status { get; protected set; }
        public APIProfile() {

        }
        public void suspend() {
            this.status = 0;
        }
        public APIProfile(string name, int isAdmin) {
            this.profileName = name;
            this.isAdmin = isAdmin;
            this.publicKey = Cryptography.CharGenerator.genID(36, Cryptography.CharGenerator.characterSet.HEX_STRING);
            this.privateKey = Cryptography.CharGenerator.genID(36, Cryptography.CharGenerator.characterSet.HEX_STRING);
            this.dateCreated = Utilities.getTodayDate().unixTimestamp;
            this.status = 1;
        }
    }
}

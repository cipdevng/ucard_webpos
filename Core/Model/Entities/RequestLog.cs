using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Entities {
    public class RequestLog : BaseEntity {
        public string transID { get; private set; }
        public string dateCreated { get; private set; }
        public string request { get; private set; }
        public string response { get; private set; }
        public string requestChecksum { get; private set; }
        public string responseChecksum { get; private set; }
    }
}

using Core.Model.DTO.Filter;
using Core.Model.DTO.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DTO {
    public class SocketData {
        public string data;
        public bool closeRequest;
    }
    public class SocketRequest {
        public Authorization Authorization { get; set; }
    }
    public class Authorization {
        public string token { get; set; }
    }
}

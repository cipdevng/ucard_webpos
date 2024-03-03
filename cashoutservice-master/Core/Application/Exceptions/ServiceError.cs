using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Exceptions {
    public class ServiceError : Exception {
        public ServiceError(string message = "A service error occured!") : base(message) { }
    }
}

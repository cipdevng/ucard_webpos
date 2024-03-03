using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.Exceptions {
    public class AuthenticationError : ApplicationException { 
        public AuthenticationError(string message = "Invalid Profile. Access denied") : base(message) { }
    }
}

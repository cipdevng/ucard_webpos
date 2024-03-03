using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.Exceptions {
    public class InputError : ApplicationException {
        public InputError(string message = "Invalid input.") : base(message) { }
    }
}

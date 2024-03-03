using Core.Application.Exceptions;
using Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Entities {
    public class BaseEntity {
        public long id { get; protected set; }
        public BaseEntity() {
            
        }
        public void cannotBeNull(params string[] objs) {
            foreach (string data in objs)
                if (data is null)
                    throw new InputError("One or more input is missing!");
        }
        public void cannotBeNullOrEmpty(params string[] objs) {
            foreach (string data in objs)
                if (string.IsNullOrEmpty(data))
                    throw new InputError("One or more input is missing or empty!");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DTO {
    public class QueryResult<T> {
        public IList<T> resultAsObject { get; set; }
        public string resultAsString { get; set; }
    }
}

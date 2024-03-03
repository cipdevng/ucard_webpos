using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Model.DTO.Internal {
    public class JobQueueProperties {
        public IModel channel { get; set; }
        public IBasicProperties config { get; set; }
        public string appID { get; set; }
    }
}

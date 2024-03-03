using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Abstraction.Worker {
    public interface IJobConsumer {
        public Task consume(BasicDeliverEventArgs arg, object obj);
    }
}

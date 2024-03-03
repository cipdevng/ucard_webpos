using Core.Model.DTO.Internal;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.Interfaces.JobQueue {
    public interface IJobQueue {
        JobQueueProperties configure(string queueName, bool persist = true, IDictionary<string, object>? headers = null);
        bool send(object payload, JobQueueProperties factory);
        bool send(string payload, JobQueueProperties factory);
    }
    public interface IQueueConnection {
        IConnection createConnection();
    }
}

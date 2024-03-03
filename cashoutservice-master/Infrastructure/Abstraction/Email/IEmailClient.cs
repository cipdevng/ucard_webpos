using Core.Model.DTO.External;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Abstraction.Email {
    public interface IEmailClient {
        Task<bool> send(MailEnvelope envelope);
    }
}

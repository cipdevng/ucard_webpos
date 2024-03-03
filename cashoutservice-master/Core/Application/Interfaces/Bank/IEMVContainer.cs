using Core.Model.Enums;
using NetCore.AutoRegisterDi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Core.Application.Interfaces.Bank {
    public interface IEMVContainer {
        Task<IEMVCardPayment?> getService(Channels channel);
    }
}

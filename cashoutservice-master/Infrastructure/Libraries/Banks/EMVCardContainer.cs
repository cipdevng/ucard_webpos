using Core.Application.Exceptions;
using Core.Application.Interfaces.Bank;
using Core.Model.Enums;
using Infrastructure.Abstraction.Socket;
using NetCore.AutoRegisterDi;

namespace Infrastructure.Libraries.Banks {
    [RegisterAsSingleton]
    public class EMVContainer : IEMVContainer {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IEMVCardPayment> _paymentProvider;
        private readonly IWebSocketManager _socket;
        public EMVContainer(IEnumerable<IEMVCardPayment> paymentProvider, IWebSocketManager socket) {
            _paymentProvider = paymentProvider;
            _socket = socket;
        }
        public async Task<IEMVCardPayment?> getService(Channels channel) {
            await _socket.sendMessage("Attempting to get service type " + channel.ToString());
            switch (channel) {
                case Channels.ARCA:
                    return _paymentProvider.OfType<ArcaImpl>().FirstOrDefault();
                case Channels.FIDESIC:
                    return _paymentProvider.OfType<Fidesic>().FirstOrDefault();
                case Channels.GRUPP:
                    return _paymentProvider.OfType<LuxByGrupp>().FirstOrDefault();
                case Channels.KIMONO:
                    return _paymentProvider.OfType<InterswitchKimono>().FirstOrDefault();
                default:
                    throw new ServiceError("Service not found!");
            }
        }
    }
}

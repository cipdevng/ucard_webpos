using Infrastructure.Libraries.Socket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Abstraction.Socket {
    public interface IWebSocketManager {
        void addConnection(string username, WebSocketConnection connection);
        void removeConnection(string username, WebSocketConnection connection);
        Task sendMessage(string message);
        Task sendMessage(string message, string username);
        Task closeAllConnections();
        Task closeConnection(string clientID);
        Task closeConnection(string clientID, WebSocketConnection connection);
    }
}

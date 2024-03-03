using Infrastructure.Abstraction.Socket;
using NetCore.AutoRegisterDi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Libraries.Socket {
    [RegisterAsSingleton]
    public class WebsocketManager : IWebSocketManager, IDisposable {
        private ConcurrentDictionary<string, List<WebSocketConnection>> _sockets = new ConcurrentDictionary<string, List<WebSocketConnection>>();
        public void addConnection(string? clientID, WebSocketConnection connection) {
            if (string.IsNullOrEmpty(clientID))
                clientID = "DefaultClients";
            _sockets.AddOrUpdate(clientID, new List<WebSocketConnection> { connection }, (a, b) => { b.Add(connection); return b; });
        }

        public async Task closeAllConnections() {
            foreach (KeyValuePair<string, List<WebSocketConnection>> data in this._sockets) {
                for (int i = 0; i < data.Value.Count; i++) {
                    await data.Value[i].closeAsync();
                }
            }
            _sockets.Clear();
            return;
        }

        public async Task closeConnection(string clientID) {
            if (string.IsNullOrEmpty(clientID))
                clientID = "DefaultClients";
            var data = this._sockets[clientID];
            for (int i = 0; i < data.Count; i++) {
                await data[i].closeAsync();
            }
            var g = new List<WebSocketConnection>();
            this._sockets.Remove(clientID, out g);
        }

        public async Task closeConnection(string clientID, WebSocketConnection connection) {
            if (string.IsNullOrEmpty(clientID))
                clientID = "DefaultClients";
            var data = this._sockets[clientID];
            await data.Find(B => B == connection).closeAsync();
            removeConnection(clientID, connection);
        }

        public void Dispose() {
            try {
                _ = closeAllConnections();
            } catch { }
        }

        public void removeConnection(string clientID, WebSocketConnection connection) {
            if (string.IsNullOrEmpty(clientID))
                clientID = "DefaultClients";
            _sockets.AddOrUpdate(clientID, new List<WebSocketConnection> { connection }, (a, b) => {
                b.Remove(connection); return b;
            });
        }

        public async Task sendMessage(string message) {
            foreach (KeyValuePair<string, List<WebSocketConnection>> data in this._sockets) {
                for (int i = 0; i < data.Value.Count; i++) {
                    var socket = data.Value[i];
                    await socket.consumeMessage(message);
                }
            }
        }

        public async Task sendMessage(string message, string clientID) {
            if (string.IsNullOrEmpty(clientID))
                clientID = "DefaultClients";
            var data = this._sockets[clientID];
            for (int i = 0; i < data.Count; i++) {
                await data[i].consumeMessage(message);
            }
        }
    }
}

using Core.Application.Interfaces.Identity;
using Core.Application.Interfaces.UseCases;
using Core.Model.DTO.Filter;
using Core.Model.DTO.Response;
using Core.Shared;
using Infrastructure.Abstraction.Socket;
using Infrastructure.DTO;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Libraries.Socket {
    public class WebSocketConnection : IDisposable {
         private readonly WebSocket _webSocket;
         public Encoding Encoding { get; }
         private bool authenticated = false;
         private string requestIdentifier;
         private readonly IAccountUseCase _identity;
         private readonly IWebSocketManager _socketManager;        
         private SocketRequest request;
         public string connectionID { get; private set; } = Cryptography.CharGenerator.genID();
         private readonly AsyncQueue<SocketData> _sendQueue = new AsyncQueue<SocketData>();
         private Task _communicationTask;
         WebResponse<object> response = new WebResponse<object>();
         public WebSocketConnection(WebSocket webSocket, IServiceProvider serviceProvider, string requestID, Encoding? encoding = null) {
             _webSocket = webSocket;
             Encoding = encoding ?? Encoding.UTF8;
             requestIdentifier = requestID;
             this.request = new SocketRequest();
             using (var scope = serviceProvider.CreateScope()) {
                try {
                    _identity = scope.ServiceProvider.GetRequiredService<IAccountUseCase>();
                    _socketManager = scope.ServiceProvider.GetRequiredService<IWebSocketManager>();
                } catch(Exception err) {
                    Debug.WriteLine(err.ToString());
                }                                  
             }
         }

         public Task handleCommunicationAsync() {
             if (_communicationTask == null) {
                 _communicationTask = Task.WhenAll(receiveTask(), sendTask());                
             }
             return _communicationTask;
         }

         public async Task consumeMessage(string data) {
             if (data.StartsWith("I/:")) {
                 await processRequest(data.Replace("I/:", string.Empty));
                 return;
             }
             _sendQueue.Enqueue(new SocketData { data = data });
         }

         public Task closeAsync() {
             if (_communicationTask == null) return closeSocketAsync();
             _sendQueue.Enqueue(new SocketData { closeRequest = true });
             return _communicationTask;
         }

         private Task closeSocketAsync() {
             return _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
         }

         private async Task receiveTask() {
             var messageSize = new byte[1024 * 10];
             var buffer = new ArraySegment<byte>(messageSize);
             while (true) {
                 try {
                     var message = await _webSocket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                     var messageObj = Encoding.Default.GetString(new ArraySegment<byte>(messageSize, 0, message.Count).ToArray());
                     await consumeMessage(messageObj);
                 } catch (Exception ex) {
                     bool canIgnoreException = (ex as WebSocketException)?.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely;
                     if (!canIgnoreException) {
                         Console.WriteLine(ex);
                     }
                 }
                 if (_webSocket.State != WebSocketState.Open) {
                     _sendQueue.EnqueueIfEmpty(new SocketData { closeRequest = true });
                     return;
                 }
             }
         }

         private async Task sendTask() {
             while (true) {
                 if (_webSocket.State != WebSocketState.Open) return;
                 SocketData[] toSend = await _sendQueue.DequeueAsync().ConfigureAwait(false);
                 foreach (var request in toSend) {
                     if (request.closeRequest) {
                         if (_webSocket.State == WebSocketState.Open) {
                             await closeSocketAsync();
                         }
                         return;
                     }
                     if (_webSocket.State != WebSocketState.Open) {
                         throw new InvalidOperationException("Write operation failed, the socket is no longer open");
                     }
                     var buffer = Encoding.UTF8.GetBytes(request.data);
                     var sendTask = _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                     await sendTask.ConfigureAwait(false);
                 }
             }
         }

         private async Task processRequest(string message) {
            if (this.authenticated)
                return;
            message = message.Trim();
            var msgs = message.Split(' ').ToList();
            var secret = msgs.Find(F => F.StartsWith("-secret"));
            secret = secret?.Replace("-secret", "")?.Trim();
            var apikey = msgs.Find(F => F.StartsWith("-apikey"))?.Replace("-apikey", "")?.Trim();
             if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(apikey)) {
                await sendFailure("Secret and API Key are required!");
                 return;
             }
            await handleAuth(apikey, secret);
         }

         private async Task handleAuth(string apikey, string secret) {
             if (this.authenticated)
                 return;
             if (string.IsNullOrEmpty(apikey) || string.IsNullOrEmpty(secret)) {
                 await sendFailure("Invalid Token Object");
             }
            if (await _identity.loadAuthenticatation(apikey, secret)) {
                this.authenticated = true;
                await sendSuccess("Authentication Successful");
                _socketManager.addConnection(null, this);
                return;
            }
            await sendFailure("Authentication Failed!");
        }        

         async Task sendFailure(string message) {
             var t = JObject.FromObject(response.fail(message)).ToString();
             await consumeMessage(t);
         }
         async Task sendSuccess(string message) {
             var t = JObject.FromObject(response.success(message)).ToString();
             await consumeMessage(t);
         }
         async Task messageObjToString(object messObj) {
             var t = JObject.FromObject(messObj).ToString();
             await consumeMessage(t);
         }
         

        public void Dispose() {
        }
    }
}

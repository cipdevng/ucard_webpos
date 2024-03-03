using Core.Application.Interfaces.Identity;
using Infrastructure.Abstraction.Socket;
using Infrastructure.Libraries.Socket;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.Controllers {    
    public class WebSocketController : ControllerBase {
        private readonly IIdentityManager identity;
        private readonly IWebSocketManager socketManager;
        private readonly IServiceProvider _provider;
        public WebSocketController(IIdentityManager identity, IWebSocketManager socketManager, IServiceProvider provider) {
            this.identity = identity;
            this.socketManager = socketManager;
            this._provider = provider;
        }

        [HttpGet("/ws/d")]
        public async Task d() {
            if (HttpContext.WebSockets.IsWebSocketRequest) {
                await handleConnection(HttpContext.WebSockets, "/d");
            } else {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task handleConnection(WebSocketManager wsManager, string route) {
            using (var ws = await wsManager.AcceptWebSocketAsync()) {
                var sender = new WebSocketConnection(ws, _provider, route);
                try {
                    await sender.consumeMessage("Welcome. Please state your token!");
                    await sender.handleCommunicationAsync();
                } finally {
                    
                }
            }
        }
    }
}

using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStack.Http;

public abstract class WebSocketManager
{
    public abstract bool IsWebSocketRequest { get; }

    public abstract void AcceptWebSocketRequest(AcceptWebSocketDelegate handler);
}

public delegate Task AcceptWebSocketDelegate(IHttpContext httpContext, WebSocket webSocket);

using System;
using HttpStack.Http;

namespace HttpStack;

internal class DefaultWebSocketManagerImp : WebSocketManager
{
    public static readonly DefaultWebSocketManagerImp Instance = new();

    public override bool IsWebSocketRequest => false;

    public override void AcceptWebSocketRequest(AcceptWebSocketDelegate handler) => throw new NotSupportedException();
}
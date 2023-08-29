using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.Collections;
using HttpStack.Http;

namespace HttpStack;

public interface IHttpContext
{
    IHttpRequest Request { get; }

    IHttpResponse Response { get; }

    IDictionary<object,object?> Items { get; }

    IServiceProvider RequestServices { get; }

    CancellationToken RequestAborted { get; }

    IFeatureCollection Features { get; }
}

public interface IHttpContext<TContext> : IHttpContext
{
    TContext InnerContext { get; }

    ValueTask LoadAsync();

    WebSocketManager WebSockets { get; }

    void SetContext(TContext context, IServiceProvider requestServices);

    void Reset();
}

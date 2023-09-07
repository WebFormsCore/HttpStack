using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.Collections;
using HttpStack.Http;

namespace HttpStack;

public interface IHttpContext
{
    IHttpRequest Request { get; }

    IHttpResponse Response { get; }

    IDictionary<object, object?> Items { get; }

    IServiceProvider RequestServices { get; }

    CancellationToken RequestAborted { get; }

    IFeatureCollection Features { get; }

    WebSocketManager WebSockets { get; }

    ClaimsPrincipal User { get; set; }

    ISession Session { get; set; }
}

public interface IHttpContext<TContext> : IHttpContext
{
    TContext InnerContext { get; }

    ValueTask LoadAsync();

    ValueTask FinalizeAsync();

    void SetContext(TContext context, IServiceProvider requestServices);

    void Reset();
}
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.Collections;
using HttpStack.Http;

namespace HttpStack;

public abstract class DefaultHttpContext<T> : IHttpContext<T>
{
    private ClaimsPrincipal? _user;
    private DefaultFeatureCollection? _features;
    private IServiceProvider? _requestServices;
    private Dictionary<object, object?>? _items;

    public abstract IHttpRequest Request { get; }
    public abstract IHttpResponse Response { get; }

    public virtual IDictionary<object, object?> Items => _items ??= new Dictionary<object, object?>();

    public virtual IServiceProvider RequestServices => _requestServices ??= EmptyServiceProvider.Instance;

    public virtual CancellationToken RequestAborted => default;

    public virtual IFeatureCollection Features => _features ??= new DefaultFeatureCollection();

    public virtual WebSocketManager WebSockets => DefaultWebSocketManager.Instance;

    public virtual ClaimsPrincipal User
    {
        get => _user ??= new ClaimsPrincipal();
        set => _user = value;
    }

    public T InnerContext { get; private set; } = default!;

    public ValueTask LoadAsync() => LoadAsyncCore();

    public virtual ValueTask FinalizeAsync() => default;

    protected virtual ValueTask LoadAsyncCore() => default;

    public void SetContext(T context, IServiceProvider requestServices)
    {
        InnerContext = context;
        _requestServices = requestServices;
        SetContextCore(context);
    }

    protected abstract void SetContextCore(T context);

    public void Reset()
    {
        InnerContext = default!;
        _user = null;
        _features?.Reset();
        _requestServices = null;
        _items?.Clear();
        ResetCore();
    }

    protected abstract void ResetCore();
}

file class EmptyServiceProvider : IServiceProvider
{
    public static readonly EmptyServiceProvider Instance = new();

    public object? GetService(Type serviceType) => null;
}

file class DefaultWebSocketManager : WebSocketManager
{
    public static readonly DefaultWebSocketManager Instance = new();

    public override bool IsWebSocketRequest => false;

    public override void AcceptWebSocketRequest(AcceptWebSocketDelegate handler) => throw new NotSupportedException();
}
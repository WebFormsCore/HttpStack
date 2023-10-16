using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.Collections;
using HttpStack.Http;

namespace HttpStack;

public abstract class BaseHttpContext<T> : IHttpContext<T>
{
    private ClaimsPrincipal? _user;
    private DefaultFeatureCollection? _features;
    private IServiceProvider? _requestServices;
    private Dictionary<object, object?>? _items;
    private ISession? _overridenSession;

    public abstract IHttpRequest Request { get; }

    public abstract IHttpResponse Response { get; }

    public virtual IDictionary<object, object?> Items => _items ??= new Dictionary<object, object?>();

    public virtual IServiceProvider RequestServices => _requestServices ??= EmptyServiceProviderImpl.Instance;

    public virtual CancellationToken RequestAborted => default;

    public virtual IFeatureCollection Features => _features ??= new DefaultFeatureCollection();

    public virtual WebSocketManager WebSockets => DefaultWebSocketManagerImp.Instance;

    protected virtual ISession? DefaultSession => null;

    public virtual ClaimsPrincipal User
    {
        get => _user ??= new ClaimsPrincipal();
        set => _user = value;
    }

    public ISession Session
    {
        get => _overridenSession ?? DefaultSession ?? DefaultSessionImpl.Instance;
        set => _overridenSession = value;
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
        _overridenSession = null;
        ResetCore();
    }

    protected abstract void ResetCore();
}

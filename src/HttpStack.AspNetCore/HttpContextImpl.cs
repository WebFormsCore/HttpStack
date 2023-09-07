using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.AspNetCore.Collections;
using HttpStack.Collections;
using Microsoft.AspNetCore.Http;
using WebSocketManager = HttpStack.Http.WebSocketManager;

namespace HttpStack.AspNetCore;

internal class HttpContextImpl : DefaultHttpContext<HttpContext>
{
    private HttpContext _httpContext = null!;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly FeatureCollectionImpl _features = new();
    private readonly SessionImpl _session = new();
    private readonly WebSocketManagerImpl _webSocketManager;

    public HttpContextImpl()
    {
        _webSocketManager = new WebSocketManagerImpl(this);
    }

    protected override void SetContextCore(HttpContext httpContext)
    {
        _httpContext = httpContext;
        _request.SetHttpRequest(httpContext.Request);
        _response.SetHttpResponse(httpContext.Response);
        _features.SetFeatureCollection(httpContext.Features);
        _session.SetHttpContext(httpContext);
        _webSocketManager.SetContext(httpContext);
    }

    protected override void ResetCore()
    {
        _httpContext = null!;
        _request.Reset();
        _response.Reset();
        _webSocketManager.Reset();
        _session.Reset();
    }

    public override ValueTask FinalizeAsync()
    {
        if (_webSocketManager.CurrentWebSocketHandler is { } task)
        {
            return new ValueTask(task);
        }

        return default;
    }

    protected override ISession DefaultSession => _session;
    public override IHttpRequest Request => _request;
    public override IHttpResponse Response => _response;
    public override IDictionary<object, object?> Items => _httpContext.Items;
    public override CancellationToken RequestAborted => _httpContext.RequestAborted;
    public override IFeatureCollection Features => _features;
    public override WebSocketManager WebSockets => _webSocketManager;

    public override ClaimsPrincipal User
    {
        get => _httpContext.User;
        set => _httpContext.User = value;
    }
}

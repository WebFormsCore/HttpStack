using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.AspNetCore.Collections;
using HttpStack.Collections;
using Microsoft.AspNetCore.Http;
using WebSocketManager = HttpStack.Http.WebSocketManager;

namespace HttpStack.AspNetCore;

internal class HttpContextImpl : IHttpContext<HttpContext>
{
    private HttpContext _httpContext = null!;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly FeatureCollectionImpl _features = new();
    private readonly WebSocketManagerImpl _webSocketManager;

    public HttpContextImpl()
    {
        _webSocketManager = new WebSocketManagerImpl(this);
    }

    public void SetContext(HttpContext httpContext, IServiceProvider requestServices)
    {
        _httpContext = httpContext;
        RequestServices = requestServices;
        _request.SetHttpRequest(httpContext.Request);
        _response.SetHttpResponse(httpContext.Response);
        _features.SetFeatureCollection(httpContext.Features);
        _webSocketManager.SetContext(httpContext);
    }

    public ValueTask LoadAsync()
    {
        return default;
    }

    public void Reset()
    {
        _httpContext = null!;
        RequestServices = null!;
        _request.Reset();
        _response.Reset();
        _webSocketManager.Reset();
    }

    public ValueTask FinalizeAsync()
    {
        if (_webSocketManager.CurrentWebSocketHandler is { } task)
        {
            return new ValueTask(task);
        }

        return default;
    }

    public HttpContext InnerContext => _httpContext;
    public IHttpRequest Request => _request;
    public IHttpResponse Response => _response;
    public IDictionary<object, object?> Items => _httpContext.Items;
    public IServiceProvider RequestServices { get; private set; } = null!;
    public CancellationToken RequestAborted => _httpContext.RequestAborted;
    public IFeatureCollection Features => _features;
    public WebSocketManager WebSockets => _webSocketManager;
    public ClaimsPrincipal User
    {
        get => _httpContext.User;
        set => _httpContext.User = value;
    }
}

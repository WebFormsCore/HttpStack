using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Collections;
using HttpStack.Http;
using Microsoft.Web.WebView2.Core;

namespace HttpStack.WebView2;

internal class HttpContextImpl : IHttpContext<WebView2Context>
{
    private WebView2Context _context;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly DefaultFeatureCollection _features = new();
    private readonly Dictionary<object, object?> _items = new();

    public void SetContext(WebView2Context context, IServiceProvider requestServices)
    {
        _context = context;
        _request.SetHttpRequest(context.Args.Request);
        _response.Initialize();
        RequestServices = requestServices;
    }

    public ValueTask LoadAsync()
    {
        return _request.LoadAsync();
    }

    public void Reset()
    {
        _features.Reset();
        _request.Reset();
        _response.Reset();
        _items.Clear();
        RequestServices = null!;
    }

    internal CoreWebView2WebResourceResponse CreateResponse() => _response.CreateResponse(_context.WebView2);

    public WebView2Context InnerContext => _context;
    public IHttpRequest Request => _request;
    public IHttpResponse Response => _response;
    public IDictionary<object, object?> Items => _items;
    public IServiceProvider RequestServices { get; private set; } = null!;
    public CancellationToken RequestAborted => default;
    public IFeatureCollection Features => _features;
    public WebSocketManager WebSockets => throw new NotSupportedException();
}

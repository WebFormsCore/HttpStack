using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Collections;

namespace HttpStack.NetHttpListener;

internal class HttpContextImpl : IHttpContext<HttpListenerContext>
{
    private HttpListenerContext _httpContext = null!;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly DefaultFeatureCollection _features = new();
    private readonly Dictionary<object, object?> _items = new();

    public async ValueTask SetContextAsync(HttpListenerContext httpContext, IServiceProvider requestServices)
    {
        _httpContext = httpContext;
        await _request.SetHttpRequestAsync(httpContext.Request);
        _response.SetHttpResponse(httpContext.Response);
        RequestServices = requestServices;
    }

    public void Reset()
    {
        _features.Reset();
        _request.Reset();
        _response.Reset();
        _items.Clear();
        _httpContext = null!;
    }

    public HttpListenerContext InnerContext => _httpContext;
    public IHttpRequest Request => _request;
    public IHttpResponse Response => _response;
    public IDictionary<object, object?> Items => _items;
    public IServiceProvider RequestServices { get; private set; } = null!;
    public CancellationToken RequestAborted => default;

    public IFeatureCollection Features => _features;
}

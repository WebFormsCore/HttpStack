using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpStack.AspNet.Collections;
using HttpStack.Collections;

namespace HttpStack.AspNet;

internal class HttpContextImpl : IHttpContext<HttpContext>
{
    private HttpContext _httpContext = null!;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly DefaultFeatureCollection _defaultFeatures = new();
    private readonly HashDictionary _items = new();

    public ValueTask SetContextAsync(HttpContext httpContext, IServiceProvider requestServices)
    {
        _httpContext = httpContext;
        _request.SetHttpRequest(httpContext.Request);
        _response.SetHttpResponse(httpContext.Response);
        _items.SetDictionary(httpContext.Items);
        RequestServices = requestServices;
        return default;
    }

    public void Reset()
    {
        DidFinishStack = false;
        _request.Reset();
        _response.Reset();
        _items.Reset();
        _defaultFeatures.Reset();
        _httpContext = null!;
    }

    public HttpContext InnerContext => _httpContext;
    public IHttpRequest Request => _request;
    public IHttpResponse Response => _response;
    public IDictionary<object, object?> Items => _items;
    public IServiceProvider RequestServices { get; private set; } = null!;
    public CancellationToken RequestAborted => _httpContext.Request.TimedOutToken;
    public IFeatureCollection Features => _defaultFeatures;
    public bool DidFinishStack { get; set; }
}

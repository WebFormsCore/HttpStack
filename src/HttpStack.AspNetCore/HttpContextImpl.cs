using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpStack.AspNetCore.Collections;
using HttpStack.Collections;
using Microsoft.AspNetCore.Http;

namespace HttpStack.AspNetCore;

internal class HttpContextImpl : IHttpContext<HttpContext>
{
    private HttpContext _httpContext = null!;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly FeatureCollectionImpl _features = new();

    public ValueTask SetContextAsync(HttpContext httpContext, IServiceProvider requestServices)
    {
        _httpContext = httpContext;
        RequestServices = requestServices;
        _request.SetHttpRequest(httpContext.Request);
        _response.SetHttpResponse(httpContext.Response);
        _features.SetFeatureCollection(httpContext.Features);
        return default;
    }

    public void Reset()
    {
        _httpContext = null!;
        RequestServices = null!;
        _request.Reset();
        _response.Reset();
    }

    public HttpContext InnerContext => _httpContext;
    public IHttpRequest Request => _request;
    public IHttpResponse Response => _response;
    public IDictionary<object, object?> Items => _httpContext.Items;
    public IServiceProvider RequestServices { get; private set; } = null!;
    public CancellationToken RequestAborted => _httpContext.RequestAborted;
    public IFeatureCollection Features => _features;
}

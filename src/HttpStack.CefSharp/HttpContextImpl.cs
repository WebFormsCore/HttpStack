using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using HttpStack.Collections;

namespace HttpStack.CefSharp;

internal struct CefContext
{
    public CefContext(IRequest request, ResourceHandler response)
    {
        Request = request;
        Response = response;
    }

    public IRequest Request { get; }

    public ResourceHandler Response { get; }
}

internal class HttpContextImpl : IHttpContext<CefContext>
{
    private CefContext _context;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly DefaultFeatureCollection _features = new();
    private readonly Dictionary<object, object?> _items = new();

    public void SetContext(CefContext context, IServiceProvider requestServices)
    {
        _context = context;
        _request.SetHttpRequest(context.Request);
        _response.SetHttpResponse(context.Response);
        RequestServices = requestServices;
    }

    public ValueTask LoadAsync()
    {
        return default;
    }

    public void Reset()
    {
        _features.Reset();
        _request.Reset();
        _response.Reset();
        _items.Clear();
        RequestServices = null!;
    }


    public CefContext InnerContext => _context;
    public IHttpRequest Request => _request;
    public IHttpResponse Response => _response;
    public IDictionary<object, object?> Items => _items;
    public IServiceProvider RequestServices { get; private set; } = null!;
    public CancellationToken RequestAborted => default;
    public IFeatureCollection Features => _features;
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.Collections;
using Microsoft.Azure.Functions.Worker.Http;

namespace HttpStack.Azure.Functions;

internal class HttpContextImpl : IHttpContext<AzureContext>
{
    private AzureContext  _requestData = default!;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly DefaultFeatureCollection _defaultFeatures = new();
    private readonly Dictionary<object, object?> _items = new();

    public void SetContext(AzureContext context, IServiceProvider requestServices)
    {
        _requestData = context;
        _request.SetHttpRequest(context.Request);

        if (context.CustomPath is {} path)
        {
            _request.Path = path;
        }

        _response.SetHttpResponse(context.Response);
        RequestServices = requestServices;
    }

    public ValueTask LoadAsync()
    {
        return _request.LoadAsync();
    }

    public void Reset()
    {
        _defaultFeatures.Reset();
        _request.Reset();
        _response.Reset();
        _items.Clear();
        _requestData = default;
        RequestServices = null!;
    }

    public AzureContext InnerContext => _requestData;
    public IHttpRequest Request => _request;
    public IHttpResponse Response => _response;
    public IDictionary<object, object?> Items => _items;
    public IServiceProvider RequestServices { get; private set; } = null!;
    public CancellationToken RequestAborted => CancellationToken.None;
    public IFeatureCollection Features => _defaultFeatures;
}

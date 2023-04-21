using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Collections;

namespace HttpStack.Owin;

internal class HttpContextImpl : IHttpContext<IDictionary<string, object>>
{
    private IDictionary<string, object> _env = null!;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly DefaultFeatureCollection _defaultFeatures = new();
    private readonly Dictionary<object, object?> _items = new();

#if NETFRAMEWORK
    private readonly HashDictionary _contextItems = new();
#endif

    public async ValueTask SetContextAsync(IDictionary<string, object> env, IServiceProvider requestServices)
    {
        _env = env;
        await _request.SetHttpRequestAsync(env);
        _response.SetHttpResponse(env);
        RequestServices = requestServices;
        RequestAborted = _env.GetOptional(OwinConstants.CallCancelled, CancellationToken.None);

#if NETFRAMEWORK
        if (_env.TryGetValue("System.Web.HttpContextBase", out var value) && value is HttpContextBase httpContext)
        {
            _contextItems.SetDictionary(httpContext.Items);
            Items = _contextItems;
        }
        else
        {
            Items = _items;
        }
#else
        Items = _items;
#endif
    }

    public void Reset()
    {
        _defaultFeatures.Reset();
        _request.Reset();
        _response.Reset();
        _items.Clear();
        _env = null!;
        RequestServices = null!;
    }

    public IDictionary<string, object> InnerContext => _env;
    public IHttpRequest Request => _request;
    public IHttpResponse Response => _response;
    public IDictionary<object, object?> Items { get; set; } = null!;
    public IServiceProvider RequestServices { get; private set; } = null!;
    public CancellationToken RequestAborted { get; private set; }
    public IFeatureCollection Features => _defaultFeatures;
}

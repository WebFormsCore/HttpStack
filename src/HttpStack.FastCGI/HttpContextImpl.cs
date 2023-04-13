using HttpStack.Collections;
using HttpStack.FastCGI.Handlers;

namespace HttpStack.FastCGI;

internal class HttpContextImpl : IHttpContext<CgiContext>
{
    private CgiContext _context = null!;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly DefaultFeatureCollection _features = new();

    public async ValueTask SetContextAsync(CgiContext context, IServiceProvider requestServices)
    {
        _context = context;
        _response.SetHttpResponse(context);
        await _request.SetHttpRequestAsync(context);
    }

    public void Reset()
    {
        _context = null!;
        _features.Reset();
    }

    public CgiContext InnerContext => _context;
    public IHttpRequest Request => _request;
    public IHttpResponse Response => _response;
    public IDictionary<object, object?> Items { get; set; } = null!;
    public IServiceProvider RequestServices { get; private set; } = null!;
    public CancellationToken RequestAborted { get; private set; }
    public IFeatureCollection Features => _features;
}

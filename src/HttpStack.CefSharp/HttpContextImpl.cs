using CefSharp;

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

internal class HttpContextImpl : BaseHttpContext<CefContext>
{
    private readonly HttpRequestImpl _request = new();
    private readonly LazyHttpRequest _lazyRequest;
    private readonly HttpResponseImpl _response = new();

    public HttpContextImpl()
    {
        _lazyRequest = new LazyHttpRequest(_request);
    }

    protected override void SetContextCore(CefContext context)
    {
        _request.SetHttpRequest(context.Request);
        _response.SetHttpResponse(context.Response);
    }

    protected override void ResetCore()
    {
        _request.Reset();
        _lazyRequest.Reset();
        _response.Reset();
    }

    public override IHttpRequest Request => _lazyRequest;
    public override IHttpResponse Response => _response;
}

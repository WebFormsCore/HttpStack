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

internal class HttpContextImpl : DefaultHttpContext<CefContext>
{
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();

    protected override void SetContextCore(CefContext context)
    {
        _request.SetHttpRequest(context.Request);
        _response.SetHttpResponse(context.Response);
    }

    protected override void ResetCore()
    {
        _request.Reset();
        _response.Reset();
    }

    public override IHttpRequest Request => _request;
    public override IHttpResponse Response => _response;
}

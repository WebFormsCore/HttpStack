using HttpStack.FastCGI.Handlers;

namespace HttpStack.FastCGI;

internal class HttpContextImpl : BaseHttpContext<CgiContext>
{
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();

    protected override void SetContextCore(CgiContext context)
    {
        _response.SetHttpResponse(context);
        _request.SetHttpRequest(context);
    }

    protected override ValueTask LoadAsyncCore()
    {
        return _request.LoadAsync();
    }

    protected override void ResetCore()
    {
        _request.Reset();
        _response.Reset();
    }

    public override IHttpRequest Request => _request;
    public override IHttpResponse Response => _response;
}

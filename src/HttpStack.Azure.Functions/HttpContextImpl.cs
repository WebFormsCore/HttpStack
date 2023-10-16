using System.Threading.Tasks;

namespace HttpStack.Azure.Functions;

internal class HttpContextImpl : BaseHttpContext<AzureContext>
{
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly LazyHttpRequest _lazyRequest;

    public HttpContextImpl()
    {
        _lazyRequest = new LazyHttpRequest(_request);
    }

    protected override void SetContextCore(AzureContext context)
    {
        _request.SetHttpRequest(context.Request);
        _response.SetHttpResponse(context.Response);

        if (context.CustomPath is {} path)
        {
            _request.Path = path;
        }
    }

    protected override ValueTask LoadAsyncCore()
    {
        return _request.LoadAsync();
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

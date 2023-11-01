namespace HttpStack.Wasm;

internal class HttpContextImpl : BaseHttpContext<WasmContext>
{
    private readonly HttpRequestImpl _request = new();
    private readonly LazyHttpRequest _lazyRequest;
    private readonly HttpResponseImpl _response = new();

    public HttpContextImpl()
    {
        _lazyRequest = new LazyHttpRequest(_request);
    }

    protected override void SetContextCore(WasmContext context)
    {
        _request.SetHttpRequest(context);
        _response.SetHeaders();
        context.Response = _response.ResponseContext;
        context.Stream = _response.MemoryStream;
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

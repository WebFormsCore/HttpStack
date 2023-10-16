using System.Net;
using System.Threading.Tasks;
using HttpStack.Http;

namespace HttpStack.NetHttpListener;

internal class HttpContextImpl : BaseHttpContext<HttpListenerContext>
{
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly WebSocketManagerImpl _webSocketManager;
    private readonly LazyHttpRequest _lazyHttpRequest;

    public HttpContextImpl()
    {
        _webSocketManager = new WebSocketManagerImpl(this);
        _lazyHttpRequest = new LazyHttpRequest(_request);
    }

    protected override void SetContextCore(HttpListenerContext httpContext)
    {
        _webSocketManager.SetContext(httpContext);
        _request.SetHttpRequest(httpContext.Request);
        _response.SetHttpResponse(httpContext.Response);
    }

    protected override ValueTask LoadAsyncCore()
    {
        return _request.LoadAsync();
    }

    protected override void ResetCore()
    {
        _webSocketManager.Reset();
        _request.Reset();
        _response.Reset();
    }

    public override IHttpRequest Request => _lazyHttpRequest;
    public override IHttpResponse Response => _response;
    public override WebSocketManager WebSockets => _webSocketManager;

    public override ValueTask FinalizeAsync()
    {
        if (_webSocketManager.CurrentWebSocketHandler is { } task)
        {
            return new ValueTask(task);
        }

        return default;
    }
}

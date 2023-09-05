using System.Net;
using System.Threading.Tasks;
using HttpStack.Http;

namespace HttpStack.NetHttpListener;

internal class HttpContextImpl : DefaultHttpContext<HttpListenerContext>
{
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly WebSocketManagerImpl _webSocketManager;

    public HttpContextImpl()
    {
        _webSocketManager = new WebSocketManagerImpl(this);
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

    public override IHttpRequest Request => _request;
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

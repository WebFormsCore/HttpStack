using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Web;
using HttpStack.AspNet.Collections;
using HttpStack.Collections;
using HttpStack.Http;

namespace HttpStack.AspNet;

internal class HttpContextImpl : DefaultHttpContext<HttpContext>
{
    private HttpContext _httpContext = null!;
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly HashDictionary _items = new();
    private readonly WebSocketManagerImpl _webSocketManager;
    private readonly SessionImpl _session = new();
    private ClaimsPrincipal? _claim;

    public HttpContextImpl()
    {
        _webSocketManager = new WebSocketManagerImpl(this);
    }

    protected override void SetContextCore(HttpContext httpContext)
    {
        _httpContext = httpContext;
        _request.SetHttpRequest(httpContext.Request);
        _response.SetHttpResponse(httpContext.Response);
        _items.SetDictionary(httpContext.Items);
        _session.SetSession(httpContext.Session);
        _webSocketManager.SetContext(httpContext);
    }

    protected override void ResetCore()
    {
        DidFinishStack = false;
        _request.Reset();
        _response.Reset();
        _items.Reset();
        _httpContext = null!;
        _webSocketManager.Reset();
        _session.Reset();
        _claim = null;
    }

    protected override ISession DefaultSession => _session;

    public bool DidFinishStack { get; set; }
    public override IHttpRequest Request => _request;
    public override IHttpResponse Response => _response;
    public override IDictionary<object, object?> Items => _items;
    public override CancellationToken RequestAborted => _httpContext.Request.TimedOutToken;
    public override WebSocketManager WebSockets => _webSocketManager;
    public override ClaimsPrincipal User
    {
        get => _claim ?? _httpContext.User as ClaimsPrincipal ?? base.User;
        set
        {
            _claim = value;
            _httpContext.User = value;
        }
    }
}

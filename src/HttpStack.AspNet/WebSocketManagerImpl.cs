using System.Web;
using System.Web.Configuration;
using HttpStack.Http;

namespace HttpStack.AspNet;

internal class WebSocketManagerImpl : WebSocketManager
{
    private readonly IHttpContext _httpContext;
    private HttpContext _context = null!;

    public WebSocketManagerImpl(IHttpContext httpContext)
    {
        _httpContext = httpContext;
    }

    public void SetContext(HttpContext context)
    {
        _context = context;
    }

    public void Reset()
    {
        _context = null!;
    }

    public override bool IsWebSocketRequest => _context.IsWebSocketRequest;

    public override void AcceptWebSocketRequest(AcceptWebSocketDelegate handler)
    {
        _context.AcceptWebSocketRequest(context => handler(_httpContext, context.WebSocket));
    }
}

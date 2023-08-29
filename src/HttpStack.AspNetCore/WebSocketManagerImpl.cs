using System.Threading.Tasks;
using HttpStack.Http;
using Microsoft.AspNetCore.Http;
using WebSocketManager = HttpStack.Http.WebSocketManager;

namespace HttpStack.AspNetCore;

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
        CurrentWebSocketHandler = null;
    }

    public Task? CurrentWebSocketHandler { get; set; }

    public override bool IsWebSocketRequest => _context.WebSockets.IsWebSocketRequest;

    public override void AcceptWebSocketRequest(AcceptWebSocketDelegate handler)
    {
        CurrentWebSocketHandler = HandleWebSocketRequest(handler);
    }

    private async Task HandleWebSocketRequest(AcceptWebSocketDelegate handler)
    {
        var webSocket = await _context.WebSockets.AcceptWebSocketAsync();

        await handler(_httpContext, webSocket);
    }
}

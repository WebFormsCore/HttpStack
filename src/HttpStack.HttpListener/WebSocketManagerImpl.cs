using System.Net;
using System.Threading.Tasks;
using HttpStack.Http;

namespace HttpStack.NetHttpListener;

internal class WebSocketManagerImpl : WebSocketManager
{
    private readonly IHttpContext _httpContext;
    private HttpListenerContext _context = null!;

    public WebSocketManagerImpl(IHttpContext httpContext)
    {
        _httpContext = httpContext;
    }

    public void SetContext(HttpListenerContext context)
    {
        _context = context;
    }

    public void Reset()
    {
        _context = null!;
        CurrentWebSocketHandler = null;
    }

    public Task? CurrentWebSocketHandler { get; set; }

    public override bool IsWebSocketRequest => _context.Request.IsWebSocketRequest;

    public override void AcceptWebSocketRequest(AcceptWebSocketDelegate handler)
    {
        CurrentWebSocketHandler = HandleWebSocketRequest(handler);
    }

    private async Task HandleWebSocketRequest(AcceptWebSocketDelegate handler)
    {
        var webSocket = await _context.AcceptWebSocketAsync(null);

        await handler(_httpContext, webSocket.WebSocket);
    }
}

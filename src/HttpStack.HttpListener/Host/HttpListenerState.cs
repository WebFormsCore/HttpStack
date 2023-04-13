using System;
using System.Net;
using System.Threading;
using HttpStack.Host;

namespace HttpStack.NetHttpListener;

internal sealed class HttpListenerState
{
    public HttpListenerState(
        HttpListener httpListener,
        IServiceProvider serviceProvider,
        IHttpStack<HttpListenerContext> httpStack,
        CancellationToken cancellationToken)
    {
        HttpListener = httpListener;
        ServiceProvider = serviceProvider;
        CancellationToken = cancellationToken;
        HttpStack = httpStack;
    }

    public HttpListener HttpListener { get; }

    public IServiceProvider ServiceProvider { get; }

    public IHttpStack<HttpListenerContext> HttpStack { get; }

    public CancellationToken CancellationToken { get; }
}
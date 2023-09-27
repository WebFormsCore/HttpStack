using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.Host;

namespace HttpStack.NetHttpListener;

internal sealed class HttpListenerState
{
    public HttpListenerState(HttpListener httpListener,
        IServiceProvider serviceProvider,
        IHttpStack<HttpListenerContext> httpStack,
        CancellationToken cancellationToken,
        TaskCompletionSource<object?> taskCompletionSource)
    {
        HttpListener = httpListener;
        ServiceProvider = serviceProvider;
        CancellationToken = cancellationToken;
        TaskCompletionSource = taskCompletionSource;
        HttpStack = httpStack;
    }

    public HttpListener HttpListener { get; }

    public IServiceProvider ServiceProvider { get; }

    public IHttpStack<HttpListenerContext> HttpStack { get; }

    public CancellationToken CancellationToken { get; }

    public TaskCompletionSource<object?> TaskCompletionSource { get; }
}
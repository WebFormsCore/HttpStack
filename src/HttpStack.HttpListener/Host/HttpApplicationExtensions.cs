using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.Host;

namespace HttpStack.NetHttpListener;

public static class HttpApplicationExtensions
{
    public static void Start(this HttpListener listener, IHttpStackBuilder app, CancellationToken cancellationToken = default)
    {
        listener.Start();

        var handler = app.CreateStack<HttpContextImpl, HttpListenerContext>();
        var state = new HttpListenerState(listener, app.Services, handler, cancellationToken);
        listener.BeginGetContext(ListenerCallback, state);
    }

    private static void ListenerCallback(IAsyncResult ar)
    {
        var state = (HttpListenerState)ar.AsyncState!;

        if (!state.HttpListener.IsListening || state.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        var httpContext = state.HttpListener.EndGetContext(ar);
        state.HttpListener.BeginGetContext(ListenerCallback, state);

        _ = Task.Run(async () =>
        {
            try
            {
                await state.HttpStack.ProcessRequestAsync(httpContext);
            }
            finally
            {
                httpContext.Response.Close();
            }
        });
    }
}

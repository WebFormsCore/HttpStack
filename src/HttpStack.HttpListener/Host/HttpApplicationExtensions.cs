using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.DependencyInjection;
using HttpStack.Host;
using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.NetHttpListener;

public static class HttpApplicationExtensions
{
    public static IHttpHostBuilder AddHttpListener(this IHttpHostBuilder builder, Action<HttpListener> configure)
    {
        builder.Services.AddSingleton<IStackService>(sp =>
        {
            var listener = new HttpListener();
            configure(listener);
            return ActivatorUtilities.CreateInstance<HttpListenerStackService>(sp, listener);
        });

        return builder;
    }

    public static IHttpHostBuilder AddHttpListener(this IHttpHostBuilder builder, string prefix)
    {
        builder.Services.AddSingleton<IStackService>(sp =>
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            return ActivatorUtilities.CreateInstance<HttpListenerStackService>(sp, listener);
        });

        return builder;
    }

    public static IHttpHostBuilder AddHttpListener(this IHttpHostBuilder builder, HttpListener listener)
    {
        builder.Services.AddSingleton<IStackService>(sp => ActivatorUtilities.CreateInstance<HttpListenerStackService>(sp, listener));
        return builder;
    }

    public static void Start(this HttpListener listener, IHttpStackBuilder app, CancellationToken cancellationToken = default)
    {
        _ = RunAsync(listener, app, cancellationToken);
    }

    public static Task RunAsync(this HttpListener listener, IHttpStackBuilder app, CancellationToken cancellationToken)
    {
        cancellationToken.Register(listener.Stop);
        listener.Start();

        var taskCompletionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = app.CreateStack<HttpContextImpl, HttpListenerContext>();
        var state = new HttpListenerState(listener, app.Services, handler, cancellationToken, taskCompletionSource);
        listener.BeginGetContext(ListenerCallback, state);

        return taskCompletionSource.Task;
    }

    private static void ListenerCallback(IAsyncResult ar)
    {
        var state = (HttpListenerState)ar.AsyncState!;

        if (!state.HttpListener.IsListening || state.CancellationToken.IsCancellationRequested)
        {
            state.TaskCompletionSource.TrySetResult(null);
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
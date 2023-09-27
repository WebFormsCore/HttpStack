using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HttpStack.NetHttpListener;

internal class HttpListenerStackService : IStackService
{
    private readonly HttpListener _listener;
    private readonly IHostApplicationLifetime _lifetime;
    private Task? _task;

    public HttpListenerStackService(HttpListener listener, IHostApplicationLifetime lifetime)
    {
        _listener = listener;
        _lifetime = lifetime;
    }

    public Task StartAsync(IHttpStackBuilder builder, CancellationToken cancellationToken = default)
    {
        _task = _listener.RunAsync(builder, _lifetime.ApplicationStopping);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return _task ?? Task.CompletedTask;
    }

    public void Dispose()
    {
        ((IDisposable)_listener).Dispose();
    }
}

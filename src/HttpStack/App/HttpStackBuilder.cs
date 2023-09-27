using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpStack;

public sealed class HttpStackBuilder : IHttpStackBuilder, IAsyncDisposable
{
    private readonly IHost? _host;
    private readonly IList<Func<MiddlewareDelegate, MiddlewareDelegate>> _components;

    private static readonly MiddlewareDelegate DefaultHandler = _ => Task.CompletedTask;
    private MiddlewareDelegate? _middleware;

    public HttpStackBuilder(IHost? host = null)
        : this(DefaultServiceProvider.Instance, host)
    {
    }

    public HttpStackBuilder(IServiceProvider services, IHost? host = null)
    {
        _host = host;
        Services = services;
        Properties = new Dictionary<string, object?>();
        _components = new List<Func<MiddlewareDelegate, MiddlewareDelegate>>();
    }

    private HttpStackBuilder(IHttpStackBuilder parent)
    {
        Services = parent.Services;
        Properties = parent.Properties;
        _components = new List<Func<MiddlewareDelegate, MiddlewareDelegate>>();
    }

    public IDictionary<string, object?> Properties { get; }

    public async Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var services = Services.GetServices<IStackService>().ToArray();

        if (services.Length == 0)
        {
            var logger = Services.GetService<ILogger<HttpStackBuilder>>();
            logger?.LogWarning("No stack services registered. The web server will not be started.");
        }

        if (_host is not null)
        {
            await _host.StartAsync(cancellationToken);
        }

        foreach (var service in services)
        {
            await service.StartAsync(this, cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var service in Services.GetServices<IStackService>())
        {
            await service.StopAsync(cancellationToken);
        }

        if (_host is not null)
        {
            await _host.StopAsync(cancellationToken);
        }
    }

    public IServiceProvider Services { get; }

    public IHttpStackBuilder New()
    {
        return new HttpStackBuilder(this);
    }

    public IHttpStackBuilder Use(Func<MiddlewareDelegate, MiddlewareDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public MiddlewareDelegate Build(MiddlewareDelegate? defaultHandler = null)
    {
        _middleware ??= _components.Reverse().Aggregate(defaultHandler ?? DefaultHandler, (current, component) => component(current));
        return _middleware;
    }

    public void Dispose()
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Services is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

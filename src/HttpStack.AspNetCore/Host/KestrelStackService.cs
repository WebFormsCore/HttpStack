using System;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.Host;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HttpStack.AspNetCore.Host;

internal sealed class KestrelStackService : IStackService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptions<SocketTransportOptions> _socketOptions;
    private readonly IOptions<KestrelServerOptions> _kestrelOptions;
    private readonly IServiceProvider _serviceProvider;
    private KestrelServer? _server;

    public KestrelStackService(
        IServiceProvider serviceProvider,
        IOptions<KestrelServerOptions>? kestrelOptions = null,
        ILoggerFactory? loggerFactory = null,
        IOptions<SocketTransportOptions>? socketOptions = null)
    {
        _serviceProvider = serviceProvider;
        _kestrelOptions = kestrelOptions ?? Options.Create(new KestrelServerOptions());
        _loggerFactory = loggerFactory ?? new NullLoggerFactory();
        _socketOptions = socketOptions ?? Options.Create(new SocketTransportOptions());
    }

    public Task StartAsync(IHttpStackBuilder builder, CancellationToken cancellationToken = default)
    {
        var stack = builder.CreateStack<HttpContextImpl, HttpContext>();
        var application = new HttpStackApplication(stack);

        var transportFactory = new SocketTransportFactory(_socketOptions, _loggerFactory);
        var options = _kestrelOptions.Value;

        options.ApplicationServices = _serviceProvider;

        _server = new KestrelServer(
            Options.Create(options),
            transportFactory,
            _loggerFactory
        );

        return _server.StartAsync(application, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return _server?.StopAsync(cancellationToken) ?? Task.CompletedTask;
    }

    public void Dispose()
    {
        _server?.Dispose();
    }
}

using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.Host;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HttpStack.AspNetCore;

public static class HttpApplicationExtensions
{
    public static IHttpStackBuilder UseStack(this IApplicationBuilder builder)
    {
        var app = new HttpStackBuilder(builder.ApplicationServices);

        builder.Use(next =>
        {
            var handler = app.CreateStack<HttpContextImpl, HttpContext>(
                context => context.RequestServices,
                context => next(Unsafe.As<HttpContextImpl>(context).InnerContext));

            return context => handler.ProcessRequestAsync(context).AsTask();
        });

        return app;
    }

    public static IApplicationBuilder UseStack(this IApplicationBuilder builder, Action<IHttpStackBuilder> configure)
    {
        var app = builder.UseStack();
        configure(app);
        return builder;
    }

    public static async Task<KestrelServer> ListenKestrelAsync(this IHttpStackBuilder app, CancellationToken cancellationToken = default)
    {
        var stack = app.CreateStack<HttpContextImpl, HttpContext>();
        var application = new HttpStackApplication(stack);

        var loggerFactory = app.Services.GetService<ILoggerFactory>() ?? new NullLoggerFactory();
        var socketOptions = app.Services.GetService<IOptions<SocketTransportOptions>>() ?? Options.Create(new SocketTransportOptions());

        var transportFactory = new SocketTransportFactory(socketOptions, loggerFactory);

        var options = app.Services.GetService<IOptions<KestrelServerOptions>>()?.Value ??
                      new KestrelServerOptions();

        options.ApplicationServices = app.Services;

        var server = new KestrelServer(
            Options.Create(options),
            transportFactory,
            loggerFactory
        );

        await server.StartAsync(application, cancellationToken);

        return server;
    }
}

using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.AspNetCore.Host;
using HttpStack.DependencyInjection;
using HttpStack.Host;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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

    public static async Task<IStackService> StartKestrelAsync(this IHttpStackBuilder app, CancellationToken cancellationToken = default)
    {
        var service = ActivatorUtilities.CreateInstance<KestrelStackService>(app.Services);
        await service.StartAsync(app, cancellationToken);
        return service;
    }

    public static IHttpHostBuilder AddKestrelStack(this IHttpHostBuilder builder, Action<KestrelServerOptions>? configure = null)
    {
        builder.Services.TryAddSingleton<IStackService, KestrelStackService>();

        if (configure is not null)
        {
            builder.Services.Configure(configure);
        }

        return builder;
    }

    public static ValueTask<HttpContextContainer> CreateStackContextAsync(this HttpContext context)
        => HttpContextContainer.CreateAsync<HttpContextImpl, HttpContext>(context, context.RequestServices);
}

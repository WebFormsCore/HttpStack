using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HttpStack.Host;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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

    public static ValueTask<HttpContextContainer> CreateStackContextAsync(this HttpContext context)
        => HttpContextContainer.CreateAsync<HttpContextImpl, HttpContext>(context, context.RequestServices);
}

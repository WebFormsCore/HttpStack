using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace HttpStack;

public static class MiddlewareExtensions
{
    public static IHttpStackBuilder UseMiddleware(this IHttpStackBuilder app, IMiddleware middleware)
    {
        return app.Use(next => context => middleware.Invoke(context, next));
    }

    public static IHttpStackBuilder UseMiddleware<
        #if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        #endif
        T>(this IHttpStackBuilder app)
        where T : IMiddleware
    {
        return app.Use(next => context =>
        {
            var middleware = ActivatorUtilities.GetServiceOrCreateInstance<T>(context.RequestServices);
            return middleware.Invoke(context, next);
        });
    }
}

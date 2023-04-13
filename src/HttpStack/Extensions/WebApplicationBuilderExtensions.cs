using System;
using System.Threading.Tasks;

namespace HttpStack;

public static class WebApplicationBuilderExtensions
{
    public static IHttpStackBuilder Run(this IHttpStackBuilder app, Func<IHttpContext, Task> middleware)
    {
        return app.Use(next =>
        {
            return context => middleware(context);
        });
    }

    public static IHttpStackBuilder Use(this IHttpStackBuilder app, Func<IHttpContext, Func<Task>, Task> middleware)
    {
        return app.Use(next =>
        {
            return context => middleware(context, () => next(context));
        });
    }

    public static IHttpStackBuilder Use(this IHttpStackBuilder app, Func<IHttpContext, MiddlewareDelegate, Task> middleware)
    {
        return app.Use(next =>
        {
            return context => middleware(context, next);
        });
    }

    public static IHttpStackBuilder MapWhen(this IHttpStackBuilder app, Func<IHttpContext, bool> condition, Action<IHttpStackBuilder> configure)
    {
        return app.Use(next =>
        {
            var builder = app.New();
            configure(builder);
            var handler = builder.Build(defaultHandler: next);

            return context => condition(context) ? handler(context) : next(context);
        });
    }

    public static IHttpStackBuilder MapPath(this IHttpStackBuilder app, PathString value, Action<IHttpStackBuilder> configure)
    {
        return app.Use(next =>
        {
            var builder = app.New();
            configure(builder);
            var handler = builder.Build(defaultHandler: next);

            return context =>
            {
                if (!context.Request.Path.StartsWithSegments(value, out var remaining))
                {
                    return next(context);
                }

                context.Request.Path = remaining;
                return handler(context);
            };
        });
    }
}

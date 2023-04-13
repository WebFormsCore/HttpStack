using System;

namespace HttpStack.Host;

public static class WebApplicationExtensions
{
    public static IHttpStack<TInnerContext> CreateStack<TContext, TInnerContext>(
        this IHttpStackBuilder stackBuilder,
        IContextScopeProvider<TInnerContext>? scopeProvider = null,
        MiddlewareDelegate? defaultHandler = null)
        where TContext : class, IHttpContext<TInnerContext>, new()
    {
        return new DefaultHttpStack<TContext, TInnerContext>(
            stackBuilder.Build(defaultHandler),
            scopeProvider ?? new DefaultContextScopeProvider<TInnerContext>(stackBuilder.Services)
        );
    }

    public static IHttpStack<TInnerContext> CreateStack<TContext, TInnerContext>(
        this IHttpStackBuilder stackBuilder,
        Func<TInnerContext, IServiceProvider> scopeGetter,
        MiddlewareDelegate? defaultHandler = null)
        where TContext : class, IHttpContext<TInnerContext>, new()
    {
        return CreateStack<TContext, TInnerContext>(
            stackBuilder,
            new RedirectContextScopeProvider<TInnerContext>(scopeGetter), defaultHandler);
    }
}

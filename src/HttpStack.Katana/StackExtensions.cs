using System;
using System.Collections.Generic;
using HttpStack.Host;
using Owin;

namespace HttpStack.Katana;

public static class StackExtensions
{
    public static IAppBuilder UseStack(this IAppBuilder builder, IHttpStackBuilder stackBuilder, IContextScopeProvider<IDictionary<string, object>>? scopeProvider = null)
    {
        return builder.Use<StackMiddleware>(
            stackBuilder,
            scopeProvider ?? new DefaultContextScopeProvider<IDictionary<string, object>>(stackBuilder.Services)
        );
    }

    public static IAppBuilder UseStack(this IAppBuilder builder, Action<IHttpStackBuilder> configure, IContextScopeProvider<IDictionary<string, object>>? scopeProvider = null)
    {
        var stackBuilder = new HttpStackBuilder();
        configure(stackBuilder);
        return builder.UseStack(stackBuilder, scopeProvider);
    }
}

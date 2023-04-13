using System.Collections.Generic;
using HttpStack.Host;

namespace HttpStack.Owin;

public static class StackExtensions
{
    public static IHttpStack<IDictionary<string, object>> CreateOwinStack(
        this IHttpStackBuilder stackBuilder,
        IContextScopeProvider<IDictionary<string, object>>? scopeProvider = null,
        MiddlewareDelegate? defaultHandler = null)
    {
        return stackBuilder.CreateStack<HttpContextImpl, IDictionary<string, object>>(scopeProvider, defaultHandler);
    }
}

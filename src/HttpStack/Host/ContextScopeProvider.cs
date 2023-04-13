using System;

namespace HttpStack.Host;

public static class ContextScopeProvider
{
    public static IContextScopeProvider<TInnerContext> Default<TInnerContext>(IServiceProvider provider)
    {
        return new DefaultContextScopeProvider<TInnerContext>(provider);
    }

    public static IContextScopeProvider<TInnerContext> Redirect<TInnerContext>(Func<TInnerContext, IServiceProvider> getter)
    {
        return new RedirectContextScopeProvider<TInnerContext>(getter);
    }
}

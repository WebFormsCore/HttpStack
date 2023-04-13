using System;
using HttpStack.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.Host;

public class DefaultContextScopeProvider<TInnerContext> : IContextScopeProvider<TInnerContext>
{
    private readonly IServiceProvider _rootServiceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public DefaultContextScopeProvider(IServiceProvider rootServiceProvider)
    {
        _rootServiceProvider = rootServiceProvider;
        _scopeFactory = rootServiceProvider.GetService<IServiceScopeFactory>() ?? new DefaultServiceScopeFactory(rootServiceProvider);
    }

    public IServiceScope CreateScope(TInnerContext context)
    {
        return _scopeFactory.CreateScope();
    }
}

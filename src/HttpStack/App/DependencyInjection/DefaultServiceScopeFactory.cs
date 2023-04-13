using System;
using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.DependencyInjection;

internal class DefaultServiceScopeFactory : IServiceScopeFactory
{
    public static readonly DefaultServiceScopeFactory Instance = new(DefaultServiceProvider.Instance);

    private readonly IServiceScope _scope;

    public DefaultServiceScopeFactory(IServiceProvider rootServiceProvider)
    {
        _scope = new DefaultServiceScope(rootServiceProvider);
    }

    public IServiceScope CreateScope() => _scope;
}

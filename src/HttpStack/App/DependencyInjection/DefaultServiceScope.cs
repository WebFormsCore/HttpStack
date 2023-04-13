using System;
using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.DependencyInjection;

internal class DefaultServiceScope : IServiceScope
{
    public DefaultServiceScope(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    public void Dispose()
    {
    }
}

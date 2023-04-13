using System;
using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.DependencyInjection;

internal class DefaultServiceProvider : IServiceProvider
{
    public static readonly DefaultServiceProvider Instance = new();

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IServiceScopeFactory))
            return DefaultServiceScopeFactory.Instance;

        return null;
    }
}

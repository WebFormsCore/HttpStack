using System;

namespace HttpStack;

internal class EmptyServiceProviderImpl : IServiceProvider
{
    public static readonly EmptyServiceProviderImpl Instance = new();

    public object? GetService(Type serviceType) => null;
}
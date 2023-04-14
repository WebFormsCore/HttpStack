using System;

namespace HttpStack.AspNet;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class HttpServiceProviderFactoryAttribute : Attribute
{
    public HttpServiceProviderFactoryAttribute(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
}

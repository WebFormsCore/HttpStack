using System;

namespace HttpStack.AspNet;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class HttpStackStartupAttribute : Attribute
{
    public HttpStackStartupAttribute(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
}

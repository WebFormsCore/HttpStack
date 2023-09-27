using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace HttpStack;

public interface IHttpStackBuilder : IHost
{
    IDictionary<string, object?> Properties { get; }

    IHttpStackBuilder New();

    IHttpStackBuilder Use(Func<MiddlewareDelegate, MiddlewareDelegate> middleware);

    MiddlewareDelegate Build(MiddlewareDelegate? defaultHandler = null);
}

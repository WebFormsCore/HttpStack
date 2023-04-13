using System;
using System.Collections.Generic;

namespace HttpStack;

public interface IHttpStackBuilder
{
    IDictionary<object, object?> Properties { get; }

    IServiceProvider Services { get; }

    IHttpStackBuilder New();

    IHttpStackBuilder Use(Func<MiddlewareDelegate, MiddlewareDelegate> middleware);

    MiddlewareDelegate Build(MiddlewareDelegate? defaultHandler = null);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HttpStack.DependencyInjection;

namespace HttpStack;

public class HttpStackBuilder : IHttpStackBuilder
{
    private readonly IList<Func<MiddlewareDelegate, MiddlewareDelegate>> _components;

    private static readonly MiddlewareDelegate DefaultHandler = _ => Task.CompletedTask;

    public HttpStackBuilder()
        : this(DefaultServiceProvider.Instance)
    {
    }

    public HttpStackBuilder(IServiceProvider services)
    {
        Services = services;
        Properties = new Dictionary<object, object?>();
        _components = new List<Func<MiddlewareDelegate, MiddlewareDelegate>>();
    }

    private HttpStackBuilder(IHttpStackBuilder parent)
    {
        Services = parent.Services;
        Properties = parent.Properties;
        _components = new List<Func<MiddlewareDelegate, MiddlewareDelegate>>();
    }

    public IDictionary<object, object?> Properties { get; }

    public IServiceProvider Services { get; }

    public IHttpStackBuilder New()
    {
        return new HttpStackBuilder(this);
    }

    public IHttpStackBuilder Use(Func<MiddlewareDelegate, MiddlewareDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public MiddlewareDelegate Build(MiddlewareDelegate? defaultHandler = null)
    {
        return _components.Reverse().Aggregate(defaultHandler ?? DefaultHandler, (current, component) => component(current));
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.Host;

public interface IHttpStack
{
    ValueTask ExecuteAsync(StackContext result);

    ValueTask DisposeAsync(StackContext result);
}

public interface IHttpStack<in TInnerContext> : IHttpStack
{
    ValueTask ProcessRequestAsync(TInnerContext context);

    StackContext CreateContext(TInnerContext innerContext);
}

public readonly struct StackContext : IAsyncDisposable
{
    private readonly IHttpStack _stack;

    public StackContext(IHttpStack stack, IHttpContext context, IServiceScope scope)
    {
        _stack = stack;
        Context = context;
        Scope = scope;
    }

    public IHttpContext Context { get; }

    public IServiceScope Scope { get; }

    public ValueTask DisposeAsync()
    {
        return _stack.DisposeAsync(this);
    }
}
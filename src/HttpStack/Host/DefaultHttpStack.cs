using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace HttpStack.Host;

public class DefaultHttpStack<TContext, TInnerContext> : IHttpStack<TInnerContext>
    where TContext : class, IHttpContext<TInnerContext>, new()
{
    private readonly ObjectPool<TContext> _pool;
    private readonly MiddlewareDelegate _middleware;
    private readonly IContextScopeProvider<TInnerContext> _scopeProvider;

    public DefaultHttpStack(MiddlewareDelegate middleware, IContextScopeProvider<TInnerContext> scopeProvider)
    {
        _middleware = middleware;
        _scopeProvider = scopeProvider;
        _pool = new DefaultObjectPool<TContext>(new ContextObjectPolicy<TContext, TInnerContext>());
    }

    protected virtual ValueTask AfterProcessRequestAsync(TContext context, TInnerContext innerContext)
    {
        return default;
    }

    public async ValueTask ProcessRequestAsync(TInnerContext innerContext)
    {
        var result = CreateContext(innerContext);
        var httpContext = Unsafe.As<TContext>(result.Context);

        await httpContext.LoadAsync();
        await _middleware(httpContext);

        await DisposeMiddlewareResultAsync(httpContext, httpContext.InnerContext, result.Scope);
    }

    public StackContext CreateContext(TInnerContext innerContext)
    {
        var httpContext = _pool.Get();
        var scope = _scopeProvider.CreateScope(innerContext);

        httpContext.SetContext(innerContext, scope.ServiceProvider);

        return new StackContext(this, httpContext, scope);
    }

    public async ValueTask ExecuteAsync(StackContext result)
    {
        if (result.Context is not TContext httpContext)
        {
            throw new InvalidOperationException("Invalid context type.");
        }

        await httpContext.LoadAsync();
        await _middleware(httpContext);
    }

    public ValueTask DisposeAsync(StackContext result)
    {
        if (result.Context is not TContext httpContext)
        {
            throw new InvalidOperationException("Invalid context type.");
        }

        return DisposeMiddlewareResultAsync(httpContext, httpContext.InnerContext, result.Scope);
    }

    private async ValueTask DisposeMiddlewareResultAsync(TContext httpContext, TInnerContext innerContext, IServiceScope scope)
    {
        try
        {
            await AfterProcessRequestAsync(httpContext, innerContext);
        }
        finally
        {
            await DisposeAsync(httpContext, scope);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async Task DisposeAsync(TContext httpContext, IServiceScope scope)
    {
        _pool.Return(httpContext);

        switch (scope)
        {
            case IAsyncDisposable disposable:
                await disposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }

}

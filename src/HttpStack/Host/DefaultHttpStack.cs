using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
        var httpContext = _pool.Get();
        var scope = _scopeProvider.CreateScope(innerContext);

        try
        {
            try
            {
                await httpContext.SetContextAsync(innerContext, scope.ServiceProvider);
                await _middleware(httpContext);
                await AfterProcessRequestAsync(httpContext, innerContext);
            }
            finally
            {
                _pool.Return(httpContext);
            }
        }
        finally
        {
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
}

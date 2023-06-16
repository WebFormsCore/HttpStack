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
    public static readonly ObjectPool<TContext> Pool = new DefaultObjectPool<TContext>(new ContextObjectPolicy<TContext, TInnerContext>());

    private readonly MiddlewareDelegate _middleware;
    private readonly IContextScopeProvider<TInnerContext> _scopeProvider;

    public DefaultHttpStack(MiddlewareDelegate middleware, IContextScopeProvider<TInnerContext> scopeProvider)
    {
        _middleware = middleware;
        _scopeProvider = scopeProvider;
    }

    protected virtual ValueTask AfterProcessRequestAsync(TContext context, TInnerContext innerContext)
    {
        return default;
    }

    public async ValueTask ProcessRequestAsync(TInnerContext innerContext)
    {
        var httpContext = Pool.Get();
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
                Pool.Return(httpContext);
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

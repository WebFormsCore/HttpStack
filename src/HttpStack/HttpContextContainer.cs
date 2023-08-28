using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HttpStack.Host;
using Microsoft.Extensions.ObjectPool;

namespace HttpStack;

public struct HttpContextContainer : IDisposable
{
    private IHttpContext _context;
    private readonly Action<IHttpContext> _dispose;

    public HttpContextContainer(IHttpContext context, Action<IHttpContext> dispose)
    {
        _context = context;
        _dispose = dispose;
    }

    public IHttpContext Context => _context;

    public void Dispose()
    {
        if (_context != null)
        {
            _dispose(_context);
            _context = null!;
        }
    }

    public static async ValueTask<HttpContextContainer> CreateAsync<TContext, TInnerContext>(TInnerContext httpContext, IServiceProvider provider)
        where TContext : class, IHttpContext<TInnerContext>, new()
    {
        var pool = DefaultHttpStack<TContext, TInnerContext>.Pool;
        var context = pool.Get();

        context.SetContext(httpContext, provider);

        await context.LoadAsync();

        return new HttpContextContainer(context, static context =>
        {
            var pool = DefaultHttpStack<TContext, TInnerContext>.Pool;
            var innerContext = Unsafe.As<TContext>(context);

            pool.Return(innerContext);
        });
    }
}

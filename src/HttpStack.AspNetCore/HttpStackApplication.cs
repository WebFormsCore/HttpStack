using System;
using System.Threading.Tasks;
using HttpStack.Host;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.ObjectPool;

namespace HttpStack.AspNetCore;

public class HttpStackApplication : IHttpApplication<DefaultHttpContext>
{
    private readonly IHttpStack<HttpContext> _stack;
    private static readonly ObjectPool<DefaultHttpContext> Pool = new DefaultObjectPool<DefaultHttpContext>(new DefaultPooledObjectPolicy<DefaultHttpContext>());

    public HttpStackApplication(IHttpStack<HttpContext> stack)
    {
        _stack = stack;
    }

    public DefaultHttpContext CreateContext(IFeatureCollection contextFeatures)
    {
        var context = Pool.Get();
        context.Initialize(contextFeatures);
        return context;
    }

    public Task ProcessRequestAsync(DefaultHttpContext context)
    {
        return _stack.ProcessRequestAsync(context).AsTask();
    }

    public void DisposeContext(DefaultHttpContext context, Exception? exception)
    {
        context.Uninitialize();
        Pool.Return(context);
    }
}

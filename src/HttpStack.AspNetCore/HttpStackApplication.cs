using System;
using System.Threading.Tasks;
using HttpStack.Host;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.ObjectPool;

namespace HttpStack.AspNetCore;

public class HttpStackApplication : IHttpApplication<Microsoft.AspNetCore.Http.DefaultHttpContext>
{
    private readonly IHttpStack<HttpContext> _stack;
    private static readonly ObjectPool<Microsoft.AspNetCore.Http.DefaultHttpContext> Pool = new DefaultObjectPool<Microsoft.AspNetCore.Http.DefaultHttpContext>(new DefaultPooledObjectPolicy<Microsoft.AspNetCore.Http.DefaultHttpContext>());

    public HttpStackApplication(IHttpStack<HttpContext> stack)
    {
        _stack = stack;
    }

    public Microsoft.AspNetCore.Http.DefaultHttpContext CreateContext(IFeatureCollection contextFeatures)
    {
        var context = Pool.Get();
        context.Initialize(contextFeatures);
        return context;
    }

    public Task ProcessRequestAsync(Microsoft.AspNetCore.Http.DefaultHttpContext context)
    {
        return _stack.ProcessRequestAsync(context).AsTask();
    }

    public void DisposeContext(Microsoft.AspNetCore.Http.DefaultHttpContext context, Exception? exception)
    {
        context.Uninitialize();
        Pool.Return(context);
    }
}

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Host;

namespace HttpStack.AspNet;

internal class HttpStackImpl : DefaultHttpStack<HttpContextImpl, HttpContext>
{
    public HttpStackImpl(MiddlewareDelegate middleware, IContextScopeProvider<HttpContext> scopeProvider)
        : base(middleware, scopeProvider)
    {
    }

    internal static Task DefaultHandler(IHttpContext context)
    {
        var innerContext = Unsafe.As<HttpContextImpl>(context);
        innerContext.DidFinishStack = true;
        return Task.CompletedTask;
    }

    protected override ValueTask AfterProcessRequestAsync(HttpContextImpl context, HttpContext innerContext)
    {
        if (context.DidFinishStack)
        {
            return default;
        }

        // Suppress the response and end the request to prevent ASP.NET from writing the response.
        innerContext.Response.Flush();
        innerContext.Response.SuppressContent = true;
        innerContext.ApplicationInstance.CompleteRequest();
        innerContext.Response.End();

        return default;
    }
}

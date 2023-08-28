using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Host;

namespace HttpStack.AspNet;

public static class Globals
{
    public const string DidFinishStackKey = "HttpStack.DidFinishStack";
    public static readonly object BoxedTrue = true;
    public static readonly object BoxedFalse = false;
}

internal class HttpStackImpl : DefaultHttpStack<HttpContextImpl, HttpContext>
{
    private readonly bool _endRequest;

    public HttpStackImpl(MiddlewareDelegate middleware, IContextScopeProvider<HttpContext> scopeProvider, bool endRequest)
        : base(middleware, scopeProvider)
    {
        _endRequest = endRequest;
    }

    internal static Task DefaultHandler(IHttpContext context)
    {
        var innerContext = Unsafe.As<HttpContextImpl>(context);
        innerContext.DidFinishStack = true;
        return Task.CompletedTask;
    }

    protected override ValueTask AfterProcessRequestAsync(HttpContextImpl context, HttpContext innerContext)
    {
        context.Items[Globals.DidFinishStackKey] = context.DidFinishStack ? Globals.BoxedTrue : Globals.BoxedFalse;

        if (!_endRequest || context.DidFinishStack)
        {
            return default;
        }

        // Suppress the response and end the request to prevent ASP.NET from writing the response.
        innerContext.Response.Flush();
        innerContext.Response.SuppressContent = true;
        innerContext.ApplicationInstance.CompleteRequest();

        return default;
    }
}

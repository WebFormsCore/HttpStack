using CefSharp;
using HttpStack.Host;

namespace HttpStack.CefSharp;

public static class HttpApplicationExtensions
{
    public static ISchemeHandlerFactory ToSchemeHandlerFactory(this IHttpStackBuilder app)
    {
        var handler = app.CreateStack<HttpContextImpl, CefContext>();
        return new HttpStackHandlerFactory(handler);
    }
}

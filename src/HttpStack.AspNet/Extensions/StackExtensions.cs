using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Host;

namespace HttpStack.AspNet;

public static class StackExtensions
{
    public static IHttpStack<HttpContext> CreateAspNetStack(
        this IHttpStackBuilder stackBuilder,
        IContextScopeProvider<HttpContext>? scopeProvider = null)
    {
        return new HttpStackImpl(
            stackBuilder.Build(HttpStackImpl.DefaultHandler),
            scopeProvider ?? new DefaultContextScopeProvider<HttpContext>(stackBuilder.Services)
        );
    }
}

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Host;

namespace HttpStack.AspNet;

public static class StackExtensions
{
    /// <summary>
    /// Creates a stack for ASP.NET.
    /// </summary>
    /// <param name="stackBuilder">The stack builder.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="endRequest"><c>true</c> to end the request if the stack was not interrupted (all the middlewares invoked next); otherwise, <c>false</c>.</param>
    /// <returns>The stack.</returns>
    public static IHttpStack<HttpContext> CreateAspNetStack(
        this IHttpStackBuilder stackBuilder,
        IContextScopeProvider<HttpContext>? scopeProvider = null,
        bool endRequest = false)
    {
        return new HttpStackImpl(
            stackBuilder.Build(HttpStackImpl.DefaultHandler),
            scopeProvider ?? new DefaultContextScopeProvider<HttpContext>(stackBuilder.Services),
            endRequest
        );
    }

    public static ValueTask<HttpContextContainer> CreateStackContextAsync(this HttpContext context, IServiceProvider provider)
        => HttpContextContainer.CreateAsync<HttpContextImpl, HttpContext>(context, provider);
}

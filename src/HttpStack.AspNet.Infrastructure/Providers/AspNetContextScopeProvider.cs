using System;
using System.Web;
using HttpStack.Host;
using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.AspNet.Providers;

internal class AspNetContextScopeProvider : IContextScopeProvider<HttpContext>
{
    private readonly IHttpServiceProviderFactory _factory;
    private readonly IServiceProvider _rootProvider;

    public AspNetContextScopeProvider(IHttpServiceProviderFactory factory, IServiceProvider rootProvider)
    {
        _factory = factory;
        _rootProvider = rootProvider;
    }

    public IServiceScope CreateScope(HttpContext context)
    {
        return _factory.CreateScope(context, _rootProvider);
    }
}

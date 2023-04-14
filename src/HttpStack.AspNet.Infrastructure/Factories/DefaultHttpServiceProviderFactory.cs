using System;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.AspNet;

internal class DefaultHttpServiceProviderFactory : IHttpServiceProviderFactory
{
    public IServiceProvider CreateRootProvider(HttpApplication application, IHttpStackStartup? startup)
    {
        var services = new ServiceCollection();
        startup?.ConfigureService(services);
        return services.BuildServiceProvider();
    }

    public IServiceScope CreateScope(HttpContext context, IServiceProvider rootScope)
    {
        return rootScope.CreateScope();
    }
}

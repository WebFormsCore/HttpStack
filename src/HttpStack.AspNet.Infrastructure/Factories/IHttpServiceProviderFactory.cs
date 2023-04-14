using System;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.AspNet;

public interface IHttpServiceProviderFactory
{
    IServiceProvider CreateRootProvider(HttpApplication application, IHttpStackStartup? startup);

    IServiceScope CreateScope(HttpContext context, IServiceProvider rootScope);
}

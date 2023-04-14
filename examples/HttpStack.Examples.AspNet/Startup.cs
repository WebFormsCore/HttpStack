using HttpStack.AspNet;
using HttpStack.Examples.AspNet;
using Microsoft.Extensions.DependencyInjection;

[assembly: HttpStackStartup(typeof(Startup))]

namespace HttpStack.Examples.AspNet;

public class Startup : IHttpStackStartup
{
    public void ConfigureService(IServiceCollection services)
    {
    }

    public void Configure(IHttpStackBuilder builder)
    {
        builder.RunWhen(
            ctx => ctx.Request.Path.StartsWithSegments("/test"),
            ctx => ctx.Response.WriteAsync("Hello World!"));
    }
}

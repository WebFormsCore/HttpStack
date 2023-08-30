using HttpStack.AspNet;
using HttpStack.Examples.AspNet;
using HttpStack.Examples.Extensions.WebSocketTime;
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
        builder.UseTime();

        builder.RunWhen(
            ctx => ctx.Request.Path.StartsWithSegments("/test"),
            ctx => ctx.Response.WriteAsync("Hello World!"));
    }
}

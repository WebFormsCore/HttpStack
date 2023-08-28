using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HttpStack;

public static class HttpApplication
{
    public static HttpApplicationBuilder CreateDefault()
    {
        var builder = new HttpApplicationBuilder();
        builder.Services.AddLogging();
        builder.Logging.AddConsole();
        return builder;
    }
}

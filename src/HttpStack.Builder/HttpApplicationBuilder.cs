using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HttpStack;

public class HttpApplicationBuilder : IHttpApplicationBuilder
{
    private IHttpStackBuilder? _application;

    public HttpApplicationBuilder()
    {
        Services = new ServiceCollection();
        Logging = new LoggingBuilder(Services);
    }

    public IServiceCollection Services { get; }

    public ILoggingBuilder Logging { get; }

    public IHttpStackBuilder Build()
    {
        if (_application != null)
        {
            return _application;
        }

        var result = new HttpStackBuilder(Services.BuildServiceProvider());
        _application = result;
        return result;
    }
}

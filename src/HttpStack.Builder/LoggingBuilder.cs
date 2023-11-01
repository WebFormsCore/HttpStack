using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HttpStack;

internal sealed class LoggingBuilder : ILoggingBuilder
{
    public LoggingBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}
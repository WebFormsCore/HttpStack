using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HttpStack;

public interface IHttpApplicationBuilder
{
    IServiceCollection Services { get; }

    ILoggingBuilder Logging { get; }

    IHttpStackBuilder Build();
}
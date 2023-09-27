using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.DependencyInjection;

public interface IHttpHostBuilder
{
    IServiceCollection Services { get; }
}

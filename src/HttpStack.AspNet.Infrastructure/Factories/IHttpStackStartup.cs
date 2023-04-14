using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.AspNet;

public interface IHttpStackStartup
{
    void ConfigureService(IServiceCollection services);

    void Configure(IHttpStackBuilder builder);
}

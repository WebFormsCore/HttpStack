using System;
using System.Threading.Tasks;
using HttpStack.Host;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace HttpStack.Azure.Functions;

public static class ServiceCollectionExtensions
{
    public static IHostBuilder ConfigureHttpStack(this IHostBuilder builder, Action<IHttpStackBuilder>? configure = null)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(provider =>
            {
                var stack = new HttpStackBuilder(provider);
                configure?.Invoke(stack);
                return stack.CreateAzureFunctionsStack();
            });
        });

        return builder;
    }

    public static Task<HttpResponseData> ExecuteHttpStackAsync(this HttpRequestData request)
    {
        return ExecuteHttpStackAsyncInner(request, null, null);
    }

    public static Task<HttpResponseData> ExecuteHttpStackAsync(this HttpRequestData request, PathString path)
    {
        return ExecuteHttpStackAsyncInner(request, path, null);
    }

    public static Task<HttpResponseData> ExecuteHttpStackAsync(this HttpRequestData request, MiddlewareDelegate middleware)
    {
        return ExecuteHttpStackAsyncInner(request, null, middleware);
    }

    public static Task<HttpResponseData> ExecuteHttpStackAsync(this HttpRequestData request, PathString path, MiddlewareDelegate middleware)
    {
        return ExecuteHttpStackAsyncInner(request, path, middleware);
    }

    private static async Task<HttpResponseData> ExecuteHttpStackAsyncInner(this HttpRequestData request, PathString? path, MiddlewareDelegate? middleware)
    {
        var stack = request.FunctionContext.InstanceServices.GetRequiredService<IHttpStack<AzureContext>>();
        var response = request.CreateResponse();
        await using var result = stack.CreateContext(new AzureContext(request, response, path));

        await stack.ExecuteAsync(result);

        if (middleware != null)
        {
            await middleware(result.Context);
        }

        return response;
    }
}

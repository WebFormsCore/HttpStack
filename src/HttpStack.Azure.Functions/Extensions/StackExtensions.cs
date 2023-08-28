using HttpStack.Host;
using Microsoft.Azure.Functions.Worker.Http;

namespace HttpStack.Azure.Functions;

public static class StackExtensions
{
    public static IHttpStack<AzureContext> CreateAzureFunctionsStack(
        this IHttpStackBuilder stackBuilder,
        IContextScopeProvider<HttpRequestData>? scopeProvider = null)
    {
        return stackBuilder.CreateStack<HttpContextImpl, AzureContext>(
            data => data.Request.FunctionContext.InstanceServices
        );
    }
}

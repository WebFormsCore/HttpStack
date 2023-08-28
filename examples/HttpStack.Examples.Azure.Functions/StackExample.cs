using System.Net;
using HttpStack.Azure.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace HttpStack.Examples.Azure.Functions;

public class HttpExample
{
    [Function("HttpExample")]
    public Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        return req.ExecuteHttpStackAsync(async context =>
        {
            await context.Response.WriteAsync("Hello World!");
        });
    }
}
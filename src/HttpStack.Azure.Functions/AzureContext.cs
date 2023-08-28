using Microsoft.Azure.Functions.Worker.Http;

namespace HttpStack.Azure.Functions;

public readonly struct AzureContext
{
    public AzureContext(HttpRequestData request, HttpResponseData response, PathString? customPath = null)
    {
        Request = request;
        Response = response;
        CustomPath = customPath;
    }

    public HttpRequestData Request { get; }

    public HttpResponseData Response { get; }

    public PathString? CustomPath { get; }
}

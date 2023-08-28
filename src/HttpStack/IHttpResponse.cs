using System.IO;
using HttpStack.Collections;

namespace HttpStack;

public interface IHttpResponse
{
    int StatusCode { get; set; }

    string? ContentType { get; set; }

    IResponseHeaderDictionary Headers { get; }

    Stream Body { get; }

    bool HasStarted { get; }

    long? ContentLength { get; set; }
}

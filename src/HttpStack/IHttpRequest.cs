using System.Collections.Generic;
using System.IO;
using HttpStack.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack;

public interface IHttpRequest
{
    string Method { get; }

    string Scheme { get; }

    string? Host { get; }

    bool IsHttps { get; }

    string Protocol { get; }

    string? ContentType { get; }

    Stream Body { get; }

    PathString Path { get; set; }

    QueryString QueryString { get; }

    IReadOnlyDictionary<string, StringValues> Query { get; }

    IFormCollection Form { get; }

    IRequestHeaderDictionary Headers { get; }

    IRequestCookieCollection Cookies { get; }
}

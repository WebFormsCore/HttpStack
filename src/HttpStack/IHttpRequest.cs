using System.Collections.Generic;
using System.IO;
using HttpStack.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack;

public interface IHttpRequest : IReadOnlyHttpRequest
{
    new string Method { get; set; }

    new string Scheme { get; set; }

    new string? Host { get; set; }

    new bool IsHttps { get; set; }

    new string Protocol { get; set; }

    new string? ContentType { get; set; }

    new Stream Body { get; set; }

    new PathString Path { get; set; }

    new QueryString QueryString { get; set; }

    new IQueryCollection Query { get; set; }

    new IFormCollection Form { get; set; }

    new IRequestHeaderDictionary Headers { get; set; }

    new IRequestCookieCollection Cookies { get; set; }
}
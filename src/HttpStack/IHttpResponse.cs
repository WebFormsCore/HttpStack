﻿using System.IO;
using HttpStack.Collections;

namespace HttpStack;

public interface IHttpResponse
{
    int StatusCode { get; set; }

    string? ContentType { get; set; }

    IResponseHeaderDictionary Headers { get; }

    IResponseCookies Cookies { get; }

    Stream Body { get; set; }

    bool HasStarted { get; }

    long? ContentLength { get; set; }
}

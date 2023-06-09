﻿using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HttpStack.Collections;
using HttpStack.FormParser;
using Microsoft.Extensions.Primitives;

namespace HttpStack.NetHttpListener;

internal class HttpRequestImpl : IHttpRequest
{
    private HttpListenerRequest _httpRequest = null!;
    private readonly NameValueDictionary _query = new();
    private readonly NameValueHeaderDictionary _headers = new();
    private readonly FormCollection _form = new();

    public async Task SetHttpRequestAsync(HttpListenerRequest httpRequest)
    {
        Path = httpRequest.Url?.AbsolutePath ?? PathString.Empty;
        _httpRequest = httpRequest;
        _query.SetNameValueCollection(httpRequest.QueryString);
        _headers.SetNameValueCollection(httpRequest.Headers);

        await _form.LoadAsync(this);
    }

    public void Reset()
    {
        _query.Reset();
        _headers.Reset();
        _form.Reset();
        _httpRequest = null!;
    }

    public string Method => _httpRequest.HttpMethod;
    public string Scheme => _httpRequest.Url?.Scheme ?? "http";
    public string? Host => _httpRequest.Url?.Host;
    public bool IsHttps => _httpRequest.IsSecureConnection;
    public string Protocol => _httpRequest.ProtocolVersion.ToString();
    public string? ContentType => _httpRequest.ContentType;
    public Stream Body => _httpRequest.InputStream;
    public PathString Path { get; set; }
    public IReadOnlyDictionary<string, StringValues> Query => _query;
    public IFormCollection Form => _form;
    public IHeaderDictionary Headers => _headers;
}

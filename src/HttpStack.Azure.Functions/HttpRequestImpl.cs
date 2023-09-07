using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HttpStack.Azure.Functions.Collections;
using HttpStack.Collections;
using HttpStack.FormParser;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Azure.Functions;

internal class HttpRequestImpl : IHttpRequest
{
    private HttpRequestData _requestData = null!;
    private readonly AzureHeaderDictionary _headers;
    private readonly RequestHeaderDictionary _requestHeaders;
    private readonly NameValueDictionary _query = new();
    private readonly FormCollection _form = new();
    private readonly RequestCookiesImpl _cookies = new();

    public HttpRequestImpl()
    {
        _headers = new AzureHeaderDictionary();
        _requestHeaders = new RequestHeaderDictionary(_headers);
    }

    public void SetHttpRequest(HttpRequestData requestData)
    {
        _requestData = requestData;
        Path = requestData.Url.AbsolutePath;
        Body = requestData.Body;
        _headers.SetHttpHeaders(requestData.Headers);
        _query.SetNameValueCollection(requestData.Query);
        _cookies.SetCookies(requestData.Cookies);
    }

    public async ValueTask LoadAsync()
    {
        await _form.LoadAsync(this);
    }

    public void Reset()
    {
        Path = default;
        Body = Stream.Null;
        _requestData = null!;
        _query.Reset();
        _headers.Reset();
        _form.Reset();
        _cookies.Reset();
    }

    public string Method => _requestData.Method;
    public string Scheme => _requestData.Url.Scheme;
    public string? Host => _requestData.Url.Host;
    public bool IsHttps => Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
    public string Protocol => "HTTP/1.1";
    public string? ContentType => Headers.ContentType;
    public Stream Body { get; set; } = Stream.Null;
    public PathString Path { get; set; }
    public QueryString QueryString => new(_requestData.Url.Query);
    public IReadOnlyDictionary<string, StringValues> Query => _query;
    public IFormCollection Form => _form;
    public IRequestHeaderDictionary Headers => _requestHeaders;
    public IRequestCookieCollection Cookies => _cookies;
}

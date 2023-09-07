using System.Collections.Generic;
using System.IO;
using System.Web;
using HttpStack.AspNet.Collections;
using HttpStack.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack.AspNet;

internal class HttpRequestImpl : IHttpRequest
{
    private HttpRequest _httpRequest = null!;
    private readonly NameValueFormCollection _form = new();
    private readonly NameValueDictionary _query = new();
    private readonly NameValueHeaderDictionary _headers;
    private readonly RequestHeaderDictionary _requestHeaders;
    private readonly RequestCookiesImpl _cookies = new();

    public HttpRequestImpl()
    {
        _headers = new NameValueHeaderDictionary();
        _requestHeaders = new RequestHeaderDictionary(_headers);
    }

    public void SetHttpRequest(HttpRequest httpRequest)
    {
        Path = httpRequest.Path;
        Body = _httpRequest.InputStream;
        _httpRequest = httpRequest;
        _form.SetHttpFileCollection(httpRequest.Files);
        _form.SetNameValueCollection(httpRequest.Form);
        _query.SetNameValueCollection(httpRequest.QueryString);
        _headers.SetNameValueCollection(httpRequest.Headers);
        _cookies.SetHttpCookieCollection(httpRequest.Cookies);
    }

    public void Reset()
    {
        Path = PathString.Empty;
        Body = Stream.Null;
        _form.Reset();
        _query.Reset();
        _headers.Reset();
        _cookies.Reset();
        _httpRequest = null!;
    }

    public string Method => _httpRequest.HttpMethod;
    public string Scheme => _httpRequest.Url.Scheme;
    public string Host => _httpRequest.Url.Host;
    public bool IsHttps => _httpRequest.IsSecureConnection;
    public string Protocol => _httpRequest.ServerVariables["SERVER_PROTOCOL"];
    public string ContentType => _httpRequest.ContentType;
    public Stream Body { get; set; } = Stream.Null;
    public PathString Path { get; set; }
    public QueryString QueryString => new(_httpRequest.Url.Query);
    public IReadOnlyDictionary<string, StringValues> Query => _query;
    public IFormCollection Form => _form;
    public IRequestHeaderDictionary Headers => _requestHeaders;
    public IRequestCookieCollection Cookies => _cookies;
}

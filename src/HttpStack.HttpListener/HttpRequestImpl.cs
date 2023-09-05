using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HttpStack.Collections;
using HttpStack.FormParser;
using HttpStack.NetHttpListener.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack.NetHttpListener;

internal class HttpRequestImpl : IHttpRequest
{
    private HttpListenerRequest _httpRequest = null!;
    private readonly NameValueDictionary _query = new();
    private readonly NameValueHeaderDictionary _headers;
    private readonly RequestHeaderDictionary _requestHeaders;
    private readonly FormCollection _form = new();
    private readonly RequestCookiesImpl _cookies = new();

    public HttpRequestImpl()
    {
        _headers = new();
        _requestHeaders = new(_headers);
    }

    public void SetHttpRequest(HttpListenerRequest httpRequest)
    {
        httpRequest.Cookies.ToString();
        Path = httpRequest.Url?.AbsolutePath ?? PathString.Empty;
        _httpRequest = httpRequest;
        _query.SetNameValueCollection(httpRequest.QueryString);
        _headers.SetNameValueCollection(httpRequest.Headers);
        _cookies.SetCookieCollection(httpRequest.Cookies);
    }

    public async ValueTask LoadAsync()
    {
        await _form.LoadAsync(this);
    }

    public void Reset()
    {
        _query.Reset();
        _headers.Reset();
        _form.Reset();
        _cookies.Reset();
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
    public QueryString QueryString => new(_httpRequest.Url?.Query ?? string.Empty);
    public IReadOnlyDictionary<string, StringValues> Query => _query;
    public IFormCollection Form => _form;
    public IRequestHeaderDictionary Headers => _requestHeaders;
    public IRequestCookieCollection Cookies => _cookies;
}

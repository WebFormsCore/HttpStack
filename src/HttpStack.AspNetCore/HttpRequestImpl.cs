using System.IO;
using System.Linq;
using HttpStack.AspNetCore.Collections;
using HttpStack.Collections;
using Microsoft.AspNetCore.Http;
using IFormCollection = HttpStack.Collections.IFormCollection;

namespace HttpStack.AspNetCore;

internal class HttpRequestImpl : IHttpRequest
{
    private HttpRequest _httpRequest = default!;
    private readonly FormCollectionDictionary _form = new();
    private readonly QueryCollectionDictionary _query = new();
    private readonly HeaderCollectionImpl _headers;
    private readonly RequestHeaderDictionary _requestHeaders;
    private readonly RequestCookiesImpl _cookies = new();

    public HttpRequestImpl()
    {
        _headers = new HeaderCollectionImpl();
        _requestHeaders = new RequestHeaderDictionary(_headers);
    }

    public void SetHttpRequest(HttpRequest httpRequest)
    {
        Path = httpRequest.Path.Value;
        _httpRequest = httpRequest;
        _query.SetQueryCollection(httpRequest.Query);
        _headers.SetHeaderDictionary(httpRequest.Headers);
        _cookies.SetRequestCookieCollection(httpRequest.Cookies);

        if (httpRequest.HasFormContentType)
        {
            _form.SetFormCollection(httpRequest.Form);
        }

        Form = _form;
        Headers = _requestHeaders;
        Cookies = _cookies;
    }

    public void Reset()
    {
        Path = PathString.Empty;
        _form.Reset();
        _query.Reset();
        _headers.Reset();
        _cookies.Reset();
        Form = null!;
        Headers = null!;
        Cookies = null!;
        _httpRequest = null!;
    }

    public string Method
    {
        get => _httpRequest.Method;
        set => _httpRequest.Method = value;
    }

    public string Scheme
    {
        get => _httpRequest.Scheme;
        set => _httpRequest.Scheme = value;
    }

    public string? Host
    {
        get => _httpRequest.Host.Value;
        set => _httpRequest.Host = value is null ? default : new HostString(value);
    }

    public bool IsHttps
    {
        get => _httpRequest.IsHttps;
        set => _httpRequest.IsHttps = value;
    }

    public string Protocol
    {
        get => _httpRequest.Protocol;
        set => _httpRequest.Protocol = value;
    }

    public string? ContentType
    {
        get => _httpRequest.ContentType;
        set => _httpRequest.ContentType = value;
    }

    public Stream Body
    {
        get => _httpRequest.Body;
        set => _httpRequest.Body = value;
    }
    public PathString Path { get; set; }

    public QueryString QueryString
    {
        get => new(_httpRequest.QueryString.Value);
        set => _httpRequest.QueryString = new Microsoft.AspNetCore.Http.QueryString(value.Value);
    }

    public HttpStack.Collections.IQueryCollection Query
    {
        get => _query;
        set
        {
            _httpRequest.Query = new Microsoft.AspNetCore.Http.QueryCollection(value.ToDictionary(x => x.Key, x => x.Value));
            _query.SetQueryCollection(_httpRequest.Query);
        }
    }

    public IFormCollection Form { get; set; } = default!;

    public IRequestHeaderDictionary Headers { get; set; } = default!;

    public IRequestCookieCollection Cookies { get; set; } = default!;
}

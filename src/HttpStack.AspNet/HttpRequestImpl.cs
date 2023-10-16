using System.Collections.Generic;
using System.IO;
using System.Web;
using HttpStack.AspNet.Collections;
using HttpStack.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack.AspNet;

internal class HttpRequestImpl : IReadOnlyHttpRequest
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
        Body = httpRequest.InputStream;
        _httpRequest = httpRequest;
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
    public IQueryCollection Query
    {
        get
        {
            _query.SetNameValueCollection(_httpRequest.QueryString);
            return _query;
        }
    }

    public IFormCollection Form
    {
        get
        {
            _form.SetHttpFileCollection(_httpRequest.Files);
            _form.SetNameValueCollection(_httpRequest.Form);
            return _form;
        }
    }

    public IRequestHeaderDictionary Headers
    {
        get
        {
            _headers.SetNameValueCollection(_httpRequest.Headers);
            return _requestHeaders;
        }
    }

    public IRequestCookieCollection Cookies
    {
        get
        {
            _cookies.SetHttpCookieCollection(_httpRequest.Cookies);
            return _cookies;
        }
    }
}

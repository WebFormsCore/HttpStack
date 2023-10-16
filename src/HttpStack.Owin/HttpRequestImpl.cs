using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Collections;
using HttpStack.Collections.Cookies;
using HttpStack.FormParser;
using HttpStack.Owin.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Owin;

internal class HttpRequestImpl : IHttpRequest
{
    private IDictionary<string, object> _env = null!;
    private readonly OwinHeaderDictionary _headers;
    private readonly RequestHeaderDictionary _requestHeaders;
    private readonly NameValueDictionary _query = new();
    private readonly FormCollection _formCollection;
    private readonly DefaultRequestCookieCollection _cookies;
#if NETFRAMEWORK
    private readonly NameValueFormCollection _formNameValue = new();
#endif

    public HttpRequestImpl()
    {
        _cookies = new DefaultRequestCookieCollection(this);
        _formCollection = new FormCollection();
        Form = _formCollection;
        _headers = new OwinHeaderDictionary();
        _requestHeaders = new RequestHeaderDictionary(_headers);
    }

    public void SetHttpRequest(IDictionary<string, object> env)
    {
        _env = env;
        var query = env.GetRequired<string>(OwinConstants.RequestQueryString);

#if NETFRAMEWORK
        // Try to reuse the existing parsed query and form data
        if (env.TryGetValue("System.Web.HttpContextBase", out var value) && value is HttpContextBase httpContext)
        {
            _query.SetNameValueCollection(httpContext.Request.QueryString);
            _formNameValue.SetNameValueCollection(httpContext.Request.Form);
            _formNameValue.SetHttpFileCollection(httpContext.Request.Files);
            Form = _formNameValue;
            return;
        }
#endif
        if (!string.IsNullOrEmpty(query))
        {
            _query.SetQueryString(query);
        }

        Query = _query;
        Form = _formCollection;
        Headers = _requestHeaders;
        Cookies = _cookies;
    }

    public async ValueTask LoadAsync()
    {
        await _formCollection.LoadAsync(this);
    }

    public void Reset()
    {
        Path = PathString.Empty;
        _env = null!;
        Method = null!;
        Scheme = null!;
        Protocol = null!;
        Body = Stream.Null;
        Path = null!;
        _query.Reset();
        _headers.Reset();
        _formCollection.Reset();
#if NETFRAMEWORK
        _formNameValue.Reset();
#endif
        _cookies.Reset();
        Form = default!;
        QueryString = default;
        Query = default!;
        Headers = default!;
        Cookies = default!;
    }

    public string Method
    {
        get => _env.GetRequired<string>(OwinConstants.RequestMethod);
        set => _env[OwinConstants.RequestMethod] = value;
    }

    public string Scheme
    {
        get => _env.GetRequired<string>(OwinConstants.RequestScheme);
        set => _env[OwinConstants.RequestScheme] = value;
    }

    public string? Host
    {
        get => _headers.TryGetValue("Host", out var host) ? host.ToString() : null;
        set => _headers["Host"] = value;
    }

    public bool IsHttps
    {
        get => Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        set => Scheme = value ? "https" : "http";
    }

    public string Protocol
    {
        get => _env.GetRequired<string>(OwinConstants.RequestProtocol);
        set => _env[OwinConstants.RequestProtocol] = value;
    }

    public string? ContentType
    {
        get => Headers.ContentType;
        set => Headers.ContentType = value;
    }

    public Stream Body
    {
        get => _env.GetRequired<Stream>(OwinConstants.RequestBody);
        set => _env[OwinConstants.RequestBody] = value;
    }

    public PathString Path
    {
        get => _env.GetRequired<string>(OwinConstants.RequestPath);
        set => _env[OwinConstants.RequestPath] = value.Value ?? string.Empty;
    }

    public QueryString QueryString
    {
        get => new(_env.GetRequired<string>(OwinConstants.RequestQueryString));
        set => _env[OwinConstants.RequestQueryString] = value.Value ?? string.Empty;
    }

    public IQueryCollection Query { get; set; } = default!;

    public IFormCollection Form { get; set; }

    public IRequestHeaderDictionary Headers { get; set; } = default!;
    public IRequestCookieCollection Cookies { get; set; } = default!;
}

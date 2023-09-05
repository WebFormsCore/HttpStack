using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Collections;
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
#if NETFRAMEWORK
    private readonly NameValueFormCollection _formNameValue = new();
#endif

    public HttpRequestImpl()
    {
        _formCollection = new();
        Form = _formCollection;
        _headers = new();
        _requestHeaders = new(_headers);
    }

    public void SetHttpRequest(IDictionary<string, object> env)
    {
        _env = env;
        Method = _env.GetRequired<string>(OwinConstants.RequestMethod);
        Scheme = _env.GetRequired<string>(OwinConstants.RequestScheme);
        Protocol = _env.GetRequired<string>(OwinConstants.RequestProtocol);
        Body = _env.GetRequired<Stream>(OwinConstants.RequestBody);
        Path = _env.GetRequired<string>(OwinConstants.RequestPath);
        _headers.SetEnvironment(_env.GetRequired<IDictionary<string, string[]>>(OwinConstants.RequestHeaders));
        Host = _headers.TryGetValue("Host", out var host) ? host.ToString() : null;

        var query = _env.GetRequired<string>(OwinConstants.RequestQueryString);

        QueryString = new QueryString(query);

#if NETFRAMEWORK
        // Try to reuse the existing parsed query and form data
        if (_env.TryGetValue("System.Web.HttpContextBase", out var value) && value is HttpContextBase httpContext)
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
        Body = null!;
        Path = null!;
        _query.Reset();
        _headers.Reset();
        _formCollection.Reset();
#if NETFRAMEWORK
        _formNameValue.Reset();
#endif
        Form = _formCollection;
        QueryString = default;
    }

    public string Method { get; private set; } = null!;
    public string Scheme { get; private set; } = null!;
    public string? Host { get; set; }
    public bool IsHttps => Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
    public string Protocol { get; private set; } = null!;
    public string? ContentType => Headers.ContentType;
    public Stream Body { get; private set; } = null!;
    public PathString Path { get; set; }
    public QueryString QueryString { get; set; }
    public IReadOnlyDictionary<string, StringValues> Query => _query;

    public IFormCollection Form { get; private set; }

    public IRequestHeaderDictionary Headers => _requestHeaders;
}

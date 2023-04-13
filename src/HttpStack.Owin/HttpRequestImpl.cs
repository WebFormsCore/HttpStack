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
    private readonly OwinHeaderDictionary _headers = new();
    private readonly NameValueDictionary _query = new();
    private readonly FormCollection _form = new();

    public async Task SetHttpRequestAsync(IDictionary<string, object> env)
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

        if (!string.IsNullOrEmpty(query))
        {
            _query.SetQueryString(query);
        }

        await _form.LoadAsync(this);
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
        _query.Clear();
        _headers.Reset();
        _form.Reset();
    }

    public string Method { get; private set; } = null!;
    public string Scheme { get; private set; } = null!;
    public string? Host { get; set; }
    public bool IsHttps => Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
    public string Protocol { get; private set; } = null!;
    public string? ContentType => Headers.ContentType;
    public Stream Body { get; private set; } = null!;
    public PathString Path { get; set; }
    public IReadOnlyDictionary<string, StringValues> Query => _query;
    public IFormCollection Form => _form;
    public IHeaderDictionary Headers => _headers;
}

using System.Web;
using HttpStack.Collections;
using HttpStack.Collections.Cookies;
using HttpStack.FastCGI.Handlers;
using HttpStack.FormParser;
using Microsoft.Extensions.Primitives;

namespace HttpStack.FastCGI;

internal class HttpRequestImpl : IHttpRequest
{
    private CgiContext _context = null!;
    private readonly NameValueDictionary _query = new();
    private readonly FormCollection _form = new();
    private readonly DefaultRequestCookieCollection _cookies;

    public HttpRequestImpl()
    {
        _cookies = new DefaultRequestCookieCollection(this);
    }

    public void SetHttpRequest(CgiContext env)
    {
        _context = env;
        Body = env.RequestStream;

        Method = env.ServerVariables.TryGetValue("REQUEST_METHOD", out var method)
            ? method
            : "GET";

        IsHttps = _context.ServerVariables.TryGetValue("HTTPS", out var https)
            ? https.Equals("on", StringComparison.OrdinalIgnoreCase)
            : Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

        Path = env.ServerVariables.TryGetValue("REQUEST_URI", out var requestUri)
            ? new PathString(requestUri)
            : PathString.Empty;

        Scheme = env.ServerVariables.TryGetValue("REQUEST_SCHEME", out var scheme)
            ? scheme
            : "http";

        Host = env.ServerVariables.TryGetValue("SERVER_NAME", out var host)
            ? host
            : null;

        Protocol = env.ServerVariables.TryGetValue("SERVER_PROTOCOL", out var protocol)
            ? protocol
            : "HTTP/1.1";

        if (env.ServerVariables.TryGetValue("QUERY_STRING", out var queryString) &&
            !string.IsNullOrEmpty(queryString))
        {
            _query.SetNameValueCollection(HttpUtility.ParseQueryString(queryString));
            QueryString = new QueryString(queryString);
        }
        else
        {
            QueryString = default;
        }

        Query = _query;
        Form = _form;
        Headers = _context.RequestHeaders;
        Cookies = _cookies;
    }

    public async ValueTask LoadAsync()
    {
        await _form.LoadAsync(this);
    }

    public void Reset()
    {
        Path = PathString.Empty;
        Body = Stream.Null;
        _context = null!;
        Scheme = null!;
        Protocol = null!;
        Path = null!;
        QueryString = default;
        _query.Reset();
        _form.Reset();
        _cookies.Reset();

        Query = default!;
        Form = default!;
        Headers = default!;
        Cookies = default!;
    }

    public string Method { get; set; } = null!;
    public string Scheme { get; set; } = null!;
    public string? Host { get; set; }
    public bool IsHttps { get; set; }

    public string Protocol { get; set; } = null!;

    public string? ContentType
    {
        get => Headers.ContentType;
        set => Headers.ContentType = value;
    }

    public Stream Body { get; set; } = Stream.Null;
    public PathString Path { get; set; }
    public QueryString QueryString { get; set; }
    public IQueryCollection Query { get; set; } = default!;
    public IFormCollection Form { get; set; } = default!;
    public IRequestHeaderDictionary Headers { get; set; } = default!;
    public IRequestCookieCollection Cookies { get; set; } = default!;
}

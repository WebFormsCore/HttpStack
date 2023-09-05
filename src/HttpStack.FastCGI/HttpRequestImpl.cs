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
        _cookies = new(this);
    }

    public void SetHttpRequest(CgiContext env)
    {
        _context = env;

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
    }

    public async ValueTask LoadAsync()
    {
        await _form.LoadAsync(this);
    }

    public void Reset()
    {
        Path = PathString.Empty;
        _context = null!;
        Scheme = null!;
        Protocol = null!;
        Path = null!;
        QueryString = default;
        _query.Reset();
        _form.Reset();
        _cookies.Reset();
    }

    public string Method => _context.ServerVariables.TryGetValue("REQUEST_METHOD", out var method) ? method : "GET";
    public string Scheme { get; private set; } = null!;
    public string? Host { get; private set; }
    public bool IsHttps => _context.ServerVariables.TryGetValue("HTTPS", out var https)
        ? https.Equals("on", StringComparison.OrdinalIgnoreCase)
        : Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
    public string Protocol { get; private set; } = null!;
    public string? ContentType => Headers.ContentType;
    public Stream Body => _context.RequestStream;
    public PathString Path { get; set; }
    public QueryString QueryString { get; set; }
    public IReadOnlyDictionary<string, StringValues> Query => _query;
    public IFormCollection Form => _form;
    public IRequestHeaderDictionary Headers => _context.RequestHeaders;
    public IRequestCookieCollection Cookies => _cookies;
}

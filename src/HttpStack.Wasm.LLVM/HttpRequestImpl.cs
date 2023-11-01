using System.Web;
using HttpStack.Collections;
using HttpStack.Collections.Cookies;
using HttpStack.Wasm.Context;

namespace HttpStack.Wasm;

internal class HttpRequestImpl : IReadOnlyHttpRequest
{
    private WasmContext _context;
    private readonly NameValueDictionary _query = new();
    private readonly HeaderDictionary _headers;
    private readonly RequestHeaderDictionary _requestHeaders;
    private readonly DefaultRequestCookieCollection _cookies;

    public HttpRequestImpl()
    {
        _cookies = new DefaultRequestCookieCollection(this);
        _headers = new HeaderDictionary();
        _requestHeaders = new RequestHeaderDictionary(_headers);
    }

    public void SetHttpRequest(WasmContext context)
    {
        _context = context;

        if (Uri.TryCreate(context.Request.Url, UriKind.RelativeOrAbsolute, out var uri))
        {
            Path = PathString.FromUriComponent(uri);
            _query.SetNameValueCollection(HttpUtility.ParseQueryString(uri.Query));
            Scheme = uri.Scheme;
            Path = uri.AbsolutePath;
            Host = uri.Host;
            QueryString = new QueryString(uri.Query);
        }
        else
        {
            Path = PathString.Empty;
            QueryString = default;
        }

        foreach (var header in context.Request.Headers)
        {
            _headers[header.Key] = header.Value;
        }

        Body = context.RequestBody;
        Form = context.Form;
    }

    public void Reset()
    {
        Path = PathString.Empty;
        _query.Reset();
        _headers.Clear();
        QueryString = default;
        _context = default;
        Scheme = "http";
        Host = null;
        _cookies.Reset();
    }

    public string Method => _context.Request.Method;
    public string Scheme { get; private set; } = "http";
    public string? Host { get; private set; }
    public bool IsHttps => Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
    public string Protocol => "HTTP/1.1";
    public string? ContentType => _headers.ContentType;
    public Stream Body { get; set; } = Stream.Null;
    public PathString Path { get; set; }
    public QueryString QueryString { get; set; }
    public IQueryCollection Query => _query;
    public IFormCollection Form { get; set; } = null!;
    public IRequestHeaderDictionary Headers => _requestHeaders;
    public IRequestCookieCollection Cookies => _cookies;
}

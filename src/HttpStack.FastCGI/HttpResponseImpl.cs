using HttpStack.Collections;
using HttpStack.Collections.Cookies;
using HttpStack.FastCGI.Handlers;

namespace HttpStack.FastCGI;

public class HttpResponseImpl : IHttpResponse
{
    private CgiContext _context = null!;
    private readonly DefaultResponseCookies _cookies;

    public HttpResponseImpl()
    {
        _cookies = new DefaultResponseCookies(this);
    }

    public void SetHttpResponse(CgiContext env)
    {
        _context = env;
        Body = env.ResponseStream;
    }

    public void Reset()
    {
        Body = Stream.Null;
        _context = null!;
        _cookies.Reset();
    }

    public int StatusCode
    {
        get => _context.ResponseStatusCode;
        set => _context.ResponseStatusCode = value;
    }

    public string? ContentType
    {
        get => Headers.ContentType;
        set => Headers.ContentType = value;
    }

    public IResponseCookies Cookies => _cookies;

    public Stream Body { get; set; } = Stream.Null;

    public bool HasStarted => _context.DidWriteHeaders;

    public long? ContentLength
    {
        get => Headers.ContentLength;
        set => Headers.ContentLength = value;
    }

    public IResponseHeaderDictionary Headers => _context.ResponseHeaders;
}

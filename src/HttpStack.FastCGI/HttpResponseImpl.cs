using HttpStack.Collections;
using HttpStack.FastCGI.Handlers;

namespace HttpStack.FastCGI;

public class HttpResponseImpl : IHttpResponse
{
    private CgiContext _context = null!;

    public void SetHttpResponse(CgiContext env)
    {
        _context = env;
    }

    public void Reset()
    {
        _context = null!;
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

    public Stream Body => _context.ResponseStream;

    public bool HasStarted => _context.DidWriteHeaders;

    public long? ContentLength
    {
        get => Headers.ContentLength;
        set => Headers.ContentLength = value;
    }

    public IHeaderDictionary Headers => _context.ResponseHeaders;
}

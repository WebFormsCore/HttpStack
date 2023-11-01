using HttpStack.Collections;
using HttpStack.Collections.Cookies;
using HttpStack.Streaming;
using HttpStack.Wasm.Context;

namespace HttpStack.Wasm;

internal class HttpResponseImpl : IHttpResponse
{
    private readonly ResponseContext _responseContext = new();
    private readonly WatchableStream _watchableStream;
    private readonly HeaderDictionary _headers;
    private readonly ResponseHeaderDictionary _responseHeaders;
    private readonly DefaultResponseCookies _cookies;

    public HttpResponseImpl()
    {
        MemoryStream = new MemoryStream();
        _watchableStream = new WatchableStream(MemoryStream);
        _cookies = new DefaultResponseCookies(this);
        _headers = new HeaderDictionary();
        _responseHeaders = new ResponseHeaderDictionary(_headers);
        Body = _watchableStream;
    }

    public MemoryStream MemoryStream { get; }

    public ResponseContext GetResponseContext()
    {
        foreach (var header in _headers)
        {
            _responseContext.Headers[header.Key] = header.Value.ToString();
        }

        return _responseContext;
    }

    public void Reset()
    {
        _responseContext.Status = 200;
        _responseContext.Headers.Clear();

        _headers.Clear();
        _cookies.Reset();
        MemoryStream.SetLength(0);

        _watchableStream.Reset(MemoryStream);
        Body = _watchableStream;
    }

    public int StatusCode
    {
        get => _responseContext.Status;
        set => _responseContext.Status = value;
    }

    public IResponseCookies Cookies => _cookies;

    public Stream Body { get; set; }

    public bool HasStarted => _watchableStream.DidWrite;

    public long? ContentLength
    {
        get => _responseHeaders.ContentLength;
        set => _responseHeaders.ContentLength = value;
    }

    public string? ContentType
    {
        get => _responseHeaders.ContentType;
        set => _responseHeaders.ContentType = value;
    }

    public IResponseHeaderDictionary Headers => _responseHeaders;
}

using HttpStack.Collections;
using HttpStack.Collections.Cookies;
using HttpStack.Streaming;
using HttpStack.Wasm.Context;

namespace HttpStack.Wasm;

internal class HttpResponseImpl : IHttpResponse
{
    private readonly WatchableStream _watchableStream;
    private readonly HeaderDictionary _headers;
    private readonly ResponseHeaderDictionary _responseHeaders;
    private readonly DefaultResponseCookies _cookies;

    public HttpResponseImpl()
    {
        Stream = new MemoryStream();
        _watchableStream = new WatchableStream(Stream);
        _cookies = new DefaultResponseCookies(this);
        _headers = new HeaderDictionary();
        _responseHeaders = new ResponseHeaderDictionary(_headers);
        Body = _watchableStream;
    }

    public MemoryStream Stream { get; }

    public ResponseContext ResponseContext { get; } = new();

    public void SetHeaders()
    {
        foreach (var header in _headers)
        {
            ResponseContext.Headers[header.Key] = header.Value.ToString();
        }
    }

    public void Reset()
    {
        ResponseContext.Headers.Clear();
        ResponseContext.Status = 200;

        _cookies.Reset();
        Body = _watchableStream;
        Stream.SetLength(0);
    }

    public int StatusCode
    {
        get => ResponseContext.Status;
        set => ResponseContext.Status = value;
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

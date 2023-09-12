using System.IO;
using CefSharp;
using HttpStack.Collections;
using HttpStack.Collections.Cookies;
using HttpStack.Streaming;

namespace HttpStack.CefSharp;

public class HttpResponseImpl : IHttpResponse
{
    private ResourceHandler _resourceHandler = null!;
    private readonly WatchableStream _stream = new();
    private readonly NameValueHeaderDictionary _headers;
    private readonly ResponseHeaderDictionary _responseHeaders;
    private readonly DefaultResponseCookies _cookies;

    public HttpResponseImpl()
    {
        _cookies = new DefaultResponseCookies(this);
        _headers = new NameValueHeaderDictionary();
        _responseHeaders = new ResponseHeaderDictionary(_headers);
    }

    public void SetHttpResponse(ResourceHandler httpResponse)
    {
        _resourceHandler = httpResponse;
        _stream.SetStream(httpResponse.Stream);
        _headers.SetNameValueCollection(httpResponse.Headers);
        Body = _stream;
    }

    public void Reset()
    {
        _headers.Reset();
        _cookies.Reset();
        _resourceHandler = null!;
        Body = Stream.Null;
    }

    public int StatusCode
    {
        get => _resourceHandler.StatusCode;
        set => _resourceHandler.StatusCode = value;
    }

    public IResponseCookies Cookies => _cookies;

    public Stream Body { get; set; } = Stream.Null;

    public bool HasStarted => _stream.DidWrite;

    public long? ContentLength
    {
        get => _resourceHandler.ResponseLength;
        set => _resourceHandler.ResponseLength = value;
    }

    public string? ContentType
    {
        get => _resourceHandler.MimeType;
        set => _resourceHandler.MimeType = value;
    }

    public IResponseHeaderDictionary Headers => _responseHeaders;
}

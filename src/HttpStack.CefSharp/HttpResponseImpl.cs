using System.IO;
using CefSharp;
using HttpStack.Collections;
using HttpStack.Streaming;

namespace HttpStack.CefSharp;

public class HttpResponseImpl : IHttpResponse
{
    private ResourceHandler _resourceHandler = null!;
    private readonly WatchableStream _stream = new();
    private readonly NameValueHeaderDictionary _headers = new();

    public void SetHttpResponse(ResourceHandler httpResponse)
    {
        _resourceHandler = httpResponse;
        _stream.SetStream(httpResponse.Stream);
        _headers.SetNameValueCollection(httpResponse.Headers);
    }

    public void Reset()
    {
        _headers.Reset();
        _resourceHandler = null!;
    }

    public int StatusCode
    {
        get => _resourceHandler.StatusCode;
        set => _resourceHandler.StatusCode = value;
    }

    public Stream Body => _stream;

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

    public IHeaderDictionary Headers => _headers;
}

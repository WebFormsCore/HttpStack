using System.IO;
using HttpStack.Collections;
using HttpStack.Streaming;
using HttpStack.WebView2.IO;
using Microsoft.IO;
using Microsoft.Web.WebView2.Core;

namespace HttpStack.WebView2;

public class HttpResponseImpl : IHttpResponse
{
    private static readonly RecyclableMemoryStreamManager Manager = new();

    private MemoryStream _memoryStream = null!;
    private readonly WatchableStream _stream = new();
    private readonly HeaderDictionary _headers;
    private readonly ResponseHeaderDictionary _responseHeaders;

    public HttpResponseImpl()
    {
        _headers = new();
        _responseHeaders = new(_headers);
    }

    internal CoreWebView2WebResourceResponse CreateResponse(CoreWebView2 webView2)
    {
        var stream = AutoCloseOnReadCompleteStream.Pool.Get();

        stream.Initialize(_memoryStream);

        return webView2.Environment.CreateWebResourceResponse(
            stream,
            StatusCode,
            ReasonPhrases.GetReasonPhrase(StatusCode),
            _headers.ToHeaderString()
        );
    }

    public void Initialize()
    {
        _memoryStream = Manager.GetStream();
        _stream.SetStream(_memoryStream);
    }

    public void Reset()
    {
        StatusCode = 200;
        _memoryStream = null!;
        _stream.Reset();
        _headers.Clear();
    }

    public int StatusCode { get; set; } = 200;

    public Stream Body => _stream;

    public bool HasStarted => _stream.DidWrite;

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

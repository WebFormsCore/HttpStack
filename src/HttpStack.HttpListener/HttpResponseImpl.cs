using System.IO;
using System.Net;
using System.Web;
using HttpStack.Collections;
using HttpStack.NetHttpListener.Collections;
using HttpStack.Streaming;

namespace HttpStack.NetHttpListener;

public class HttpResponseImpl : IHttpResponse
{
    private HttpListenerResponse _httpResponse = null!;
    private readonly WatchableStream _body = new();
    private readonly NameValueHeaderDictionary _headers;
    private readonly ResponseHeaderDictionary _responseHeaders;
    private readonly ResponseCookiesImpl _cookies = new();

    public HttpResponseImpl()
    {
        _headers = new NameValueHeaderDictionary();
        _responseHeaders = new ResponseHeaderDictionary(_headers);
    }

    public void SetHttpResponse(HttpListenerResponse httpResponse)
    {
        _httpResponse = httpResponse;
        _body.SetStream(httpResponse.OutputStream);
        _headers.SetNameValueCollection(httpResponse.Headers);
        _cookies.SetCookieCollection(httpResponse.Cookies);
        Body = _body;
    }

    public void Reset()
    {
        Body = Stream.Null;
        _headers.Reset();
        _body.Reset();
        _cookies.Reset();
        _httpResponse = null!;
    }

    public int StatusCode
    {
        get => _httpResponse.StatusCode;
        set => _httpResponse.StatusCode = value;
    }

    public IResponseCookies Cookies => _cookies;
    
    public Stream Body { get; set; } = Stream.Null;

    public bool HasStarted => _body.DidWrite;

    public long? ContentLength
    {
        get => _httpResponse.ContentLength64;
        set => _httpResponse.ContentLength64 = value ?? 0;
    }

    public string? ContentType
    {
        get => _httpResponse.ContentType;
        set => _httpResponse.ContentType = value;
    }

    public IResponseHeaderDictionary Headers => _responseHeaders;
}

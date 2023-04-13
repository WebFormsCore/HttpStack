using System.IO;
using System.Net;
using System.Web;
using HttpStack.Collections;
using HttpStack.Streaming;

namespace HttpStack.NetHttpListener;

public class HttpResponseImpl : IHttpResponse
{
    private HttpListenerResponse _httpResponse = null!;
    private readonly WatchableStream _body = new();
    private readonly NameValueHeaderDictionary _headers = new();

    public void SetHttpResponse(HttpListenerResponse httpResponse)
    {
        _httpResponse = httpResponse;
        _body.SetStream(httpResponse.OutputStream);
        _headers.SetNameValueCollection(httpResponse.Headers);
    }

    public void Reset()
    {
        _headers.Reset();
        _body.Reset();
        _httpResponse = null!;
    }

    public int StatusCode
    {
        get => _httpResponse.StatusCode;
        set => _httpResponse.StatusCode = value;
    }

    public Stream Body => _body;

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

    public IHeaderDictionary Headers => _headers;
}

using System.IO;
using System.Web;
using HttpStack.AspNet.Collections;
using HttpStack.Collections;

namespace HttpStack.AspNet;

public class HttpResponseImpl : IHttpResponse
{
    private HttpResponse _httpResponse = null!;
    private readonly NameValueHeaderDictionary _headers;
    private readonly ResponseHeaderDictionary _responseHeaders;

    public HttpResponseImpl()
    {
        _headers = new();
        _responseHeaders = new(_headers);
    }

    public void SetHttpResponse(HttpResponse httpResponse)
    {
        _httpResponse = httpResponse;
        _headers.SetNameValueCollection(httpResponse.Headers);
    }

    public void Reset()
    {
        _headers.Reset();
        _httpResponse = null!;
    }

    public int StatusCode
    {
        get => _httpResponse.StatusCode;
        set => _httpResponse.StatusCode = value;
    }

    public string? ContentType
    {
        get => _httpResponse.ContentType;
        set => _httpResponse.ContentType = value;
    }

    public IResponseHeaderDictionary Headers => _responseHeaders;

    public Stream Body => _httpResponse.OutputStream;

    public bool HasStarted => _httpResponse.HeadersWritten;

    public long? ContentLength
    {
        get => Headers.ContentLength;
        set => Headers.ContentLength = value;
    }
}

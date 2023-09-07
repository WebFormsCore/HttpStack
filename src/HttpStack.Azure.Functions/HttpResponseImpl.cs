using System.IO;
using System.Net;
using HttpStack.Azure.Functions.Collections;
using HttpStack.Collections;
using HttpStack.Streaming;
using Microsoft.Azure.Functions.Worker.Http;

namespace HttpStack.Azure.Functions;

public class HttpResponseImpl : IHttpResponse
{
    private HttpResponseData _responseData = null!;
    private readonly AzureHeaderDictionary _headers;
    private readonly ResponseHeaderDictionary _responseHeaders;
    private readonly WatchableStream _body = new();
    private readonly ResponseCookiesImpl _cookies = new();

    public HttpResponseImpl()
    {
        _headers = new AzureHeaderDictionary();
        _responseHeaders = new ResponseHeaderDictionary(_headers);
    }

    public void SetHttpResponse(HttpResponseData responseData)
    {
        _responseData = responseData;
        _body.SetStream(responseData.Body);
        _headers.SetHttpHeaders(responseData.Headers);
        _cookies.SetCookies(responseData.Cookies);
    }

    public void Reset()
    {
        _body.Reset();
        _headers.Reset();
        _cookies.Reset();
        _responseData = null!;
    }

    public int StatusCode
    {
        get => (int) _responseData.StatusCode;
        set => _responseData.StatusCode = (HttpStatusCode) value;
    }

    public string? ContentType
    {
        get => Headers.ContentType;
        set => Headers.ContentType = value;
    }

    public Stream Body
    {
        get => _body;
        set => _body.SetStream(value);
    }

    public bool HasStarted => _body.DidWrite;

    public long? ContentLength
    {
        get => Headers.ContentLength;
        set => Headers.ContentLength = value;
    }

    public IResponseHeaderDictionary Headers => _responseHeaders;

    public IResponseCookies Cookies => _cookies;
}

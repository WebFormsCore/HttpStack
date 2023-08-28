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

    public HttpResponseImpl()
    {
        _headers = new();
        _responseHeaders = new(_headers);
    }

    public void SetHttpResponse(HttpResponseData responseData)
    {
        _responseData = responseData;
        _body.SetStream(_responseData.Body);
        _headers.SetHttpHeaders(_responseData.Headers);
    }

    public void Reset()
    {
        _body.Reset();
        _headers.Reset();
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

    public Stream Body => _body;

    public bool HasStarted => _body.DidWrite;

    public long? ContentLength
    {
        get => Headers.ContentLength;
        set => Headers.ContentLength = value;
    }

    public IResponseHeaderDictionary Headers => _responseHeaders;
}

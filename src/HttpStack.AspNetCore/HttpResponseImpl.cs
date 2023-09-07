using System.IO;
using System.Web;
using HttpStack.AspNetCore.Collections;
using HttpStack.Collections;
using Microsoft.AspNetCore.Http;
using IHeaderDictionary = HttpStack.Collections.IHeaderDictionary;

namespace HttpStack.AspNetCore;

public class HttpResponseImpl : IHttpResponse
{
    private HttpResponse _httpResponse = default!;
    private readonly HeaderCollectionImpl _headers;
    private readonly ResponseHeaderDictionary _responseHeaders;
    private readonly ResponseCookiesImpl _cookies = new();

    public HttpResponseImpl()
    {
        _headers = new HeaderCollectionImpl();
        _responseHeaders = new ResponseHeaderDictionary(_headers);
    }

    public void SetHttpResponse(HttpResponse httpResponse)
    {
        _httpResponse = httpResponse;
        _headers.SetHeaderDictionary(httpResponse.Headers);
        _cookies.SetResponseCookies(httpResponse.Cookies);
    }

    public void Reset()
    {
        _httpResponse = null!;
        _headers.Reset();
        _cookies.Reset();
    }

    public int StatusCode
    {
        get => _httpResponse.StatusCode;
        set => _httpResponse.StatusCode = value;
    }

    public string? ContentType
    {
        get => _httpResponse.ContentType;
#pragma warning disable CS8601 // In ASP.NET Core 7.0 this is nullable
        set => _httpResponse.ContentType = value;
#pragma warning restore CS8601
    }

    public IResponseHeaderDictionary Headers => _responseHeaders;

    public Stream Body
    {
        get => _httpResponse.Body;
        set => _httpResponse.Body = value;
    }

    public bool HasStarted => _httpResponse.HasStarted;

    public long? ContentLength
    {
        get => _httpResponse.ContentLength;
        set => _httpResponse.ContentLength = value;
    }

    public IResponseCookies Cookies => _cookies;
}

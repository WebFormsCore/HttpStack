using System.Collections.Generic;
using System.IO;
using System.Web;
using HttpStack.AspNetCore.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using IFormCollection = HttpStack.Collections.IFormCollection;
using IHeaderDictionary = HttpStack.Collections.IHeaderDictionary;

namespace HttpStack.AspNetCore;

internal class HttpRequestImpl : IHttpRequest
{
    private HttpRequest _httpRequest = default!;
    private readonly FormCollectionDictionary _form = new();
    private readonly QueryCollectionDictionary _query = new();
    private readonly HeaderCollectionImpl _headers = new();

    public void SetHttpRequest(HttpRequest httpRequest)
    {
        Path = httpRequest.Path.Value;
        _httpRequest = httpRequest;
        _query.SetQueryCollection(httpRequest.Query);
        _headers.SetHeaderDictionary(httpRequest.Headers);

        if (httpRequest.HasFormContentType)
        {
            _form.SetFormCollection(httpRequest.Form);
        }
    }

    public void Reset()
    {
        Path = PathString.Empty;
        _form.Reset();
        _query.Reset();
        _headers.Reset();
        _httpRequest = null!;
    }

    public string Method => _httpRequest.Method;
    public string Scheme => _httpRequest.Scheme;
    public string Host => _httpRequest.Host.Host;
    public bool IsHttps => _httpRequest.IsHttps;
    public string Protocol => _httpRequest.Protocol;
    public string? ContentType => _httpRequest.ContentType;
    public Stream Body => _httpRequest.Body;
    public PathString Path { get; set; }
    public IReadOnlyDictionary<string, StringValues> Query => _query;
    public IFormCollection Form => _form;
    public IHeaderDictionary Headers => _headers;
}

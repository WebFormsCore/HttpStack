using System.Collections.Generic;
using System.IO;
using System.Web;
using HttpStack.AspNet.Collections;
using HttpStack.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack.AspNet;

internal class HttpRequestImpl : IHttpRequest
{
    private HttpRequest _httpRequest = null!;
    private readonly FileCollection _form = new();
    private readonly NameValueDictionary _query = new();
    private readonly NameValueHeaderDictionary _headers = new();

    public void SetHttpRequest(HttpRequest httpRequest)
    {
        Path = httpRequest.Path;
        _httpRequest = httpRequest;
        _form.SetNameValueCollection(httpRequest.Form);
        _query.SetNameValueCollection(httpRequest.QueryString);
        _headers.SetNameValueCollection(httpRequest.Headers);
    }

    public void Reset()
    {
        Path = PathString.Empty;
        _form.Reset();
        _query.Reset();
        _headers.Reset();
        _httpRequest = null!;
    }

    public string Method => _httpRequest.HttpMethod;
    public string Scheme => _httpRequest.Url.Scheme;
    public string Host => _httpRequest.Url.Host;
    public bool IsHttps => _httpRequest.IsSecureConnection;
    public string Protocol => _httpRequest.ServerVariables["SERVER_PROTOCOL"];
    public string ContentType => _httpRequest.ContentType;
    public Stream Body => _httpRequest.InputStream;
    public PathString Path { get; set; }
    public IReadOnlyDictionary<string, StringValues> Query => _query;
    public IFormCollection Form => _form;
    public IHeaderDictionary Headers => _headers;
}

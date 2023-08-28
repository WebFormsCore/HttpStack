using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Collections;
using HttpStack.FormParser;
using Microsoft.Extensions.Primitives;
using Microsoft.IO;
using Microsoft.Web.WebView2.Core;

namespace HttpStack.WebView2;

internal class HttpRequestImpl : IHttpRequest
{
    private static readonly RecyclableMemoryStreamManager Manager = new();

    private readonly NameValueDictionary _query = new();
    private readonly HeaderDictionary _headers;
    private readonly RequestHeaderDictionary _requestHeaders;
    private readonly FormCollection _form = new();

    public HttpRequestImpl()
    {
        _headers = new();
        _requestHeaders = new(_headers);
    }

    public void SetHttpRequest(CoreWebView2WebResourceRequest httpRequest)
    {
        foreach (var header in httpRequest.Headers)
        {
            _headers.Add(header.Key, header.Value);
        }

        Method = httpRequest.Method;
        Body = Manager.GetStream();
        httpRequest.Content?.CopyTo(Body);
        Body.Position = 0;

        if (Uri.TryCreate(httpRequest.Uri, UriKind.RelativeOrAbsolute, out var uri))
        {
            Path = PathString.FromUriComponent(uri);
            _query.SetNameValueCollection(HttpUtility.ParseQueryString(uri.Query));
            Scheme = uri.Scheme;
            Path = uri.AbsolutePath;
            Host = uri.Host;
        }
        else
        {
            Path = PathString.Empty;
        }
    }

    public async ValueTask LoadAsync()
    {
        await _form.LoadAsync(this);
    }

    public void Reset()
    {
        Path = PathString.Empty;
        _query.Reset();
        _headers.Clear();
        _form.Reset();
        Scheme = "http";
        Host = null;
        Method = null!;
        Body.Dispose();
        Body = null!;
    }

    public string Method { get; private set; } = null!;
    public string Scheme { get; private set; } = "http";
    public string? Host { get; private set; }
    public bool IsHttps => Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
    public string Protocol => "HTTP/1.1";
    public string? ContentType => _headers["Content-Type"];
    public Stream Body { get; private set; } = null!;
    public PathString Path { get; set; }
    public IReadOnlyDictionary<string, StringValues> Query => _query;
    public IFormCollection Form => _form;
    public IRequestHeaderDictionary Headers => _requestHeaders;
}

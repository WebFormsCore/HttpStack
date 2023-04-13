using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using CefSharp;
using HttpStack.Collections;
using HttpStack.Forms;
using Microsoft.Extensions.Primitives;

namespace HttpStack.CefSharp;

internal class HttpRequestImpl : IHttpRequest
{
    private IRequest _request = null!;
    private readonly NameValueDictionary _query = new();
    private readonly NameValueHeaderDictionary _headers = new();
    private readonly FormCollection _form = new();

    public void SetHttpRequest(IRequest httpRequest)
    {
        _request = httpRequest;
        _headers.SetNameValueCollection(httpRequest.Headers);

        if (Uri.TryCreate(httpRequest.Url, UriKind.RelativeOrAbsolute, out var uri))
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

        InitializeForm(httpRequest);
    }

    private void InitializeForm(IRequest httpRequest)
    {
        if (httpRequest.GetHeaderByName("Content-Type") is not { } contentType)
        {
            return;
        }

        if (contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
        {
            InitializeUrlEncodedForm(httpRequest);
        }
        else if (contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            InitializeFormData(httpRequest, contentType);
        }
    }

    private void InitializeFormData(IRequest httpRequest, string contentType)
    {
        Span<byte> boundary = stackalloc byte[128];
        var length = MultipartFormParser.GetBoundary(contentType, boundary);

        if (length == 0)
        {
            return;
        }

        using var parser = new MultipartFormParser(_form, boundary.Slice(0, length));

        foreach (var element in httpRequest.PostData.Elements)
        {
            switch (element.Type)
            {
                case PostDataElementType.Empty:
                    break;
                case PostDataElementType.Bytes:
                    parser.ParseBytes(element.Bytes);
                    break;
                case PostDataElementType.File:
                    parser.SetLocalFileAsBody(element.File, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void InitializeUrlEncodedForm(IRequest httpRequest)
    {
        foreach (var postDataElement in httpRequest.PostData.Elements)
        {
            if (postDataElement.Type != PostDataElementType.Bytes)
            {
                continue;
            }

            var bytes = postDataElement.Bytes;
            var str = Encoding.UTF8.GetString(bytes);
            var current = HttpUtility.ParseQueryString(str);

            _form.Add(current);
        }
    }

    public void Reset()
    {
        Path = PathString.Empty;
        _query.Reset();
        _headers.Reset();
        _form.Reset();
        _request = null!;
        Scheme = "http";
    }

    public string Method => _request.Method;
    public string Scheme { get; private set; } = "http";
    public string? Host { get; private set; }
    public bool IsHttps => Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
    public string Protocol => "HTTP/1.1";
    public string? ContentType => _request.Headers["Content-Type"];
    public Stream Body => Stream.Null;
    public PathString Path { get; set; }
    public IReadOnlyDictionary<string, StringValues> Query => _query;
    public IFormCollection Form => _form;
    public IHeaderDictionary Headers => _headers;
}

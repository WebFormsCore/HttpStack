using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using HttpStack.Collections;
using HttpStack.Collections.Cookies;
using HttpStack.Owin.Collections;
using HttpStack.Streaming;

namespace HttpStack.Owin;

public class HttpResponseImpl : IHttpResponse
{
    private IDictionary<string, object> _env = null!;
    private readonly OwinHeaderDictionary _headers;
    private readonly ResponseHeaderDictionary _responseHeaders;
    private readonly WatchableStream _body = new();
    private bool? _sentHeaders;
    private readonly DefaultResponseCookies _cookies;

    public HttpResponseImpl()
    {
        _cookies = new(this);
        _headers = new();
        _responseHeaders = new(_headers);
    }

    public void SetHttpResponse(IDictionary<string, object> env)
    {
        _env = env;
        _body.SetStream(_env.GetRequired<Stream>(OwinConstants.ResponseBody));
        _headers.SetEnvironment(_env.GetRequired<IDictionary<string, string[]>>(OwinConstants.ResponseHeaders));

        WatchHeaders();
    }

    public void Reset()
    {
        _body.Reset();
        _headers.Reset();
        _env = null!;
        _sentHeaders = null;
        _cookies.Reset();
    }

    private void WatchHeaders()
    {
        var onSendingHeaders = _env.GetOptional<Action<Action<object>, object>>(OwinConstants.CommonKeys.OnSendingHeaders);

        if (onSendingHeaders == null)
        {
            return;
        }

        _sentHeaders = false;

        onSendingHeaders.Invoke(static state =>
        {
            if (state is HttpResponseImpl response)
            {
                response._sentHeaders = true;
            }
        }, this);
    }

    public int StatusCode
    {
        get => _env.GetOptional(OwinConstants.ResponseStatusCode, 200);
        set => _env[OwinConstants.ResponseStatusCode] = value;
    }

    public string? ContentType
    {
        get => Headers.ContentType;
        set => Headers.ContentType = value;
    }

    public IResponseCookies Cookies => _cookies;

    public Stream Body => _body;

    public bool HasStarted => _sentHeaders ?? _body.DidWrite;

    public long? ContentLength
    {
        get => Headers.ContentLength;
        set => Headers.ContentLength = value;
    }

    public IResponseHeaderDictionary Headers => _responseHeaders;
}

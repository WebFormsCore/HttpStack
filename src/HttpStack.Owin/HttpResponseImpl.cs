using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using HttpStack.Collections;
using HttpStack.Owin.Collections;
using HttpStack.Streaming;

namespace HttpStack.Owin;

public class HttpResponseImpl : IHttpResponse
{
    private IDictionary<string, object> _env = null!;
    private readonly OwinHeaderDictionary _headers = new();
    private readonly WatchableStream _body = new();
    private bool? _sentHeaders;

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

    public Stream Body => _body;

    public bool HasStarted => _sentHeaders ?? _body.DidWrite;

    public long? ContentLength
    {
        get => Headers.ContentLength;
        set => Headers.ContentLength = value;
    }

    public IHeaderDictionary Headers => _headers;
}

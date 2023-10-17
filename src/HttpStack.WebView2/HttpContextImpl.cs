using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Collections;
using HttpStack.Http;
using Microsoft.Web.WebView2.Core;

namespace HttpStack.WebView2;

internal class HttpContextImpl : BaseHttpContext<WebView2Context>
{
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private readonly LazyHttpRequest _lazyRequest;

    public HttpContextImpl()
    {
        _lazyRequest = new LazyHttpRequest(_request);
    }

    protected override void SetContextCore(WebView2Context context)
    {
        _request.SetHttpRequest(context.Args.Request);
        _response.Initialize();
    }

    protected override ValueTask LoadAsyncCore()
    {
        return _request.LoadAsync();
    }

    protected override void ResetCore()
    {
        _request.Reset();
        _response.Reset();
    }

    internal CoreWebView2WebResourceResponse CreateResponse() => _response.CreateResponse(InnerContext.WebView2);
    public override IHttpRequest Request => _lazyRequest;
    public override IHttpResponse Response => _response;
}

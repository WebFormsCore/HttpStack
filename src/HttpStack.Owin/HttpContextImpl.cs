using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Collections;

namespace HttpStack.Owin;

internal class HttpContextImpl : BaseHttpContext<IDictionary<string, object>>
{
    private readonly HttpRequestImpl _request = new();
    private readonly HttpResponseImpl _response = new();
    private CancellationToken _requestAborted;

#if NETFRAMEWORK
    private readonly HashDictionary _contextItems = new();
    private IDictionary<object, object?> _items = null!;

    public override IDictionary<object, object?> Items => _items;
#endif

    public override IHttpRequest Request => _request;
    public override IHttpResponse Response => _response;
    public override CancellationToken RequestAborted => _requestAborted;

    protected override void SetContextCore(IDictionary<string, object> env)
    {
        _request.SetHttpRequest(env);
        _response.SetHttpResponse(env);
        _requestAborted = env.GetOptional(OwinConstants.CallCancelled, CancellationToken.None);

#if NETFRAMEWORK
        if (env.TryGetValue("System.Web.HttpContextBase", out var value) && value is HttpContextBase httpContext)
        {
            _contextItems.SetDictionary(httpContext.Items);
            _items = _contextItems;
        }
        else
        {
            _items = base.Items;
        }
#endif
    }

    protected override ValueTask LoadAsyncCore()
    {
        return _request.LoadAsync();
    }

    protected override void ResetCore()
    {
        _request.Reset();
        _response.Reset();
        _requestAborted = default;
    }
}

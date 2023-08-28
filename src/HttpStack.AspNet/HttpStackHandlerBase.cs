using System;
using System.Threading.Tasks;
using System.Web;
using HttpStack.Host;

namespace HttpStack.AspNet;

public abstract class HttpStackHandlerBase : IHttpAsyncHandler
{
    private readonly EventHandlerTaskAsyncHelper _wrapper;
    private readonly IHttpStack<HttpContext> _stack;

    protected HttpStackHandlerBase()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        _stack = CreateStackBuilder().CreateAspNetStack();
        _wrapper = new EventHandlerTaskAsyncHelper(ExecuteStackAsync);
    }

    protected abstract IHttpStackBuilder CreateStackBuilder();

    private Task ExecuteStackAsync(object sender, EventArgs e)
    {
        if (sender is not HttpApplication { Context: { } context })
        {
            return Task.CompletedTask;
        }

        return _stack.ProcessRequestAsync(context).AsTask();
    }

    public bool IsReusable => true;

    public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback? cb, object? extraData)
    {
        return _wrapper.BeginEventHandler(context, EventArgs.Empty, cb, extraData);
    }

    public void EndProcessRequest(IAsyncResult result)
    {
        _wrapper.EndEventHandler(result);
    }

    public void ProcessRequest(HttpContext context)
    {
        EndProcessRequest(BeginProcessRequest(context, null, null));
    }
}

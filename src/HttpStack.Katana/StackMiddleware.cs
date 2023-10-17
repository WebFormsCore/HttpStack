using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HttpStack.Host;
using HttpStack.Owin;

namespace HttpStack.Katana;

using AppFunc = Func<IDictionary<string, object>, Task>;

internal class StackMiddleware
{
    private readonly IHttpStack<IDictionary<string, object>> _stack;

    public StackMiddleware(AppFunc next,IHttpStackBuilder stack,  IContextScopeProvider<IDictionary<string, object>> scopeProvider)
    {
        _stack = stack.CreateOwinStack(scopeProvider, context => next(Unsafe.As<HttpContextImpl>(context).InnerContext));
    }

    public Task Invoke(IDictionary<string, object> environment)
    {
        return _stack.ProcessRequestAsync(environment).AsTask();
    }
}

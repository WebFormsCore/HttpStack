using Microsoft.Extensions.ObjectPool;

namespace HttpStack.Host;

internal class ContextObjectPolicy<TContext, TInnerContext> : PooledObjectPolicy<TContext>
    where TContext : class, IHttpContext<TInnerContext>, new()
{
    public override TContext Create()
    {
        return new TContext();
    }

    public override bool Return(TContext obj)
    {
        obj.Reset();
        return true;
    }
}

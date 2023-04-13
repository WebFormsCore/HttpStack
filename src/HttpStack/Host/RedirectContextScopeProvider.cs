using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace HttpStack.Host;

public class RedirectContextScopeProvider<T> : IContextScopeProvider<T>
{
    private readonly ObjectPool<ServiceScope> _pool = new DefaultObjectPool<ServiceScope>(new ServiceScopePooledObjectPolicy());

    private readonly Func<T, IServiceProvider> _getterFactory;

    public RedirectContextScopeProvider(Func<T, IServiceProvider> getterFactory)
    {
        _getterFactory = getterFactory;
    }

    public IServiceScope CreateScope(T context)
    {
        var scope = _pool.Get();
        scope.Pool = _pool;
        scope.ServiceProvider = _getterFactory(context);
        return scope;
    }

    private class ServiceScopePooledObjectPolicy : IPooledObjectPolicy<ServiceScope>
    {
        public ServiceScope Create()
        {
            return new ServiceScope();
        }

        public bool Return(ServiceScope obj)
        {
            obj.ServiceProvider = null!;
            obj.Pool = null;
            return true;
        }
    }


    private class ServiceScope : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; set; } = null!;

        public ObjectPool<ServiceScope>? Pool { get; set; }

        public void Dispose()
        {
            Pool?.Return(this);
        }
    }
}

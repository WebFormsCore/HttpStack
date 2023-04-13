using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;

namespace HttpStack.FastCGI.Handlers;

internal class RequestPooledObjectPolicy : IPooledObjectPolicy<CgiContext>
{
    public CgiContext Create()
    {
        return new CgiContext();
    }

    public bool Return(CgiContext obj)
    {
        if (obj.State is not RequestState.End)
        {
            throw new InvalidOperationException("Request is not in the end state.");
        }

        obj.Reset();
        return true;
    }
}

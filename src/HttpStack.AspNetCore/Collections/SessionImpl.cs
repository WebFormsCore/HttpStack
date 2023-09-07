using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStack.AspNet.Collections;

internal class SessionImpl : ISession
{
    private Microsoft.AspNetCore.Http.ISession _session = null!;

    public void SetSession(Microsoft.AspNetCore.Http.ISession session)
    {
        _session = session;
    }

    public void Reset()
    {
        _session = null!;
    }

    public bool IsAvailable => _session.IsAvailable;

    public string Id => _session.Id;

    public IEnumerable<string> Keys => _session.Keys;

    public Task LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        return _session.LoadAsync(cancellationToken);
    }

    public Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        return _session.CommitAsync(cancellationToken);
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
    {
        return _session.TryGetValue(key, out value);
    }

    public void Set(string key, byte[] value)
    {
        _session.Set(key, value);
    }

    public void Remove(string key)
    {
        _session.Remove(key);
    }

    public void Clear()
    {
        _session.Clear();
    }
}

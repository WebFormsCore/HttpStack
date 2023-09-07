using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace HttpStack.AspNet.Collections;

internal class SessionImpl : ISession, IObjectSession
{
    private HttpSessionState _session = null!;

    public void SetSession(HttpSessionState session)
    {
        _session = session;
    }

    public void Reset()
    {
        _session = null!;
    }

    public bool IsAvailable => true;

    public string Id => _session.SessionID;

    public IEnumerable<string> Keys => _session.Keys.Cast<string>();

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
    {
        value = (byte[]?)_session[key];
        return value != null;
    }

    public void Set(string key, byte[] value)
    {
        _session[key] = value;
    }

    public void Remove(string key)
    {
        _session.Remove(key);
    }

    public void Clear()
    {
        _session.Clear();
    }

    void IObjectSession.Set(string key, object? value)
    {
        _session[key] = value;
    }

    bool IObjectSession.TryGetValue(string key, [NotNullWhen(true)] out object? value)
    {
        value = _session[key];
        return value != null;
    }
}

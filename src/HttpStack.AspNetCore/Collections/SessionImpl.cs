using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HttpStack.AspNetCore.Collections;

internal class SessionImpl : ISession
{
    private HttpContext _context = null!;

    public void SetHttpContext(HttpContext session)
    {
        _context = session;
    }

    public void Reset()
    {
        _context = null!;
    }

    public bool IsAvailable => _context.Session.IsAvailable;

    public string Id => _context.Session.Id;

    public IEnumerable<string> Keys => _context.Session.Keys;

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return _context.Session.LoadAsync(cancellationToken);
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return _context.Session.CommitAsync(cancellationToken);
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
    {
        return _context.Session.TryGetValue(key, out value);
    }

    public void Set(string key, byte[] value)
    {
        _context.Session.Set(key, value);
    }

    public void Remove(string key)
    {
        _context.Session.Remove(key);
    }

    public void Clear()
    {
        _context.Session.Clear();
    }
}

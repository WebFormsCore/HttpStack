using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStack;

internal class DefaultSessionImpl : ISession
{
    public static readonly DefaultSessionImpl Instance = new();

    public bool IsAvailable => false;

    public string Id => string.Empty;

    public IEnumerable<string> Keys => Enumerable.Empty<string>();
    public Task LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        return Task.CompletedTask;
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
    {
        value = null;
        return false;
    }

    public void Set(string key, byte[] value)
    {
        throw new NotSupportedException("Session is not available.");
    }

    public void Remove(string key)
    {
    }

    public void Clear()
    {
    }
}
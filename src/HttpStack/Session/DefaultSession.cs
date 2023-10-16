using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStack;

public class DefaultSession : ISession
{
	private Dictionary<string, byte[]> _session = new();

	public bool IsAvailable { get; set; } = true;

	public string Id { get; set; } = string.Empty;

	public IEnumerable<string> Keys => _session.Keys;

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
		return _session.TryGetValue(key, out value);
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
}

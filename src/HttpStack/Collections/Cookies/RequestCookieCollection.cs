using System.Collections;
using System.Collections.Generic;

namespace HttpStack.Collections.Cookies;

public class RequestCookieCollection : IRequestCookieCollection
{
	private readonly Dictionary<string, string> _dictionary = new();

	public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_dictionary).GetEnumerator();
	}

	public int Count => _dictionary.Count;

	public ICollection<string> Keys => _dictionary.Keys;

	public bool ContainsKey(string key)
	{
		return _dictionary.ContainsKey(key);
	}

	public bool TryGetValue(string key, out string? value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	public string? this[string key] => _dictionary[key];
}

using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Collections;

public class QueryCollection : IQueryCollection
{
	private readonly Dictionary<string, StringValues> _dictionary = new();

	public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_dictionary).GetEnumerator();
	}

	public int Count => _dictionary.Count;

	public bool ContainsKey(string key)
	{
		return _dictionary.ContainsKey(key);
	}

	public bool TryGetValue(string key, out StringValues value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	public StringValues this[string key] => _dictionary[key];

	public IEnumerable<string> Keys => _dictionary.Keys;

	public IEnumerable<StringValues> Values => _dictionary.Values;
}

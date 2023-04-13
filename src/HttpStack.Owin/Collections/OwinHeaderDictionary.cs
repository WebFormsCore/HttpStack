using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HttpStack.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Owin.Collections;

internal class OwinHeaderDictionary : IHeaderDictionary
{
    private IDictionary<string, string[]> _dictionary = null!;

    public void SetEnvironment(IDictionary<string, string[]> nameValueCollection)
    {
        _dictionary = nameValueCollection;
    }

    public void Reset()
    {
        _dictionary = null!;
    }

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return _dictionary
            .Select(kv => new KeyValuePair<string, StringValues>(kv.Key, new StringValues(kv.Value)))
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<string, StringValues> item)
    {
        _dictionary.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public bool Contains(KeyValuePair<string, StringValues> item)
    {
        return _dictionary.ContainsKey(item.Key) && _dictionary[item.Key] == item.Value;
    }

    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
    {
        foreach (var item in this)
        {
            array[arrayIndex++] = item;
        }
    }

    public bool Remove(KeyValuePair<string, StringValues> item)
    {
        if (Contains(item))
        {
            _dictionary.Remove(item.Key);
            return true;
        }

        return false;
    }

    public int Count => _dictionary.Count;
    public bool IsReadOnly => false;
    public bool ContainsKey(string key)
    {
        return _dictionary.ContainsKey(key);
    }

    public void Add(string key, StringValues value)
    {
        _dictionary.Add(key, value);
    }

    public bool Remove(string key)
    {
        if (ContainsKey(key))
        {
            _dictionary.Remove(key);
            return true;
        }

        return false;
    }

    public bool TryGetValue(string key, out StringValues value)
    {
        if (ContainsKey(key))
        {
            value = _dictionary[key];
            return true;
        }

        value = default;
        return false;
    }

    public StringValues this[string key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    public ICollection<string> Keys => _dictionary.Keys;

    public ICollection<StringValues> Values => _dictionary.Values
        .Select(v => new StringValues(v))
        .ToList();

    public string? ContentType
    {
        get => GetSingleValue("Content-Type");
        set => SetSingleValue("Content-Type", value);
    }

    public long? ContentLength
    {
        get => long.TryParse(GetSingleValue("Content-Length"), out var contentLength) ? contentLength : null;
        set => SetSingleValue("Content-Length", value?.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string? GetSingleValue(string key)
    {
        return _dictionary.TryGetValue(key, out var values) && values.Length > 0 ? values[0] : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetSingleValue(string key, string? value)
    {
        if (value is null)
        {
            _dictionary.Remove(key);
        }
        else
        {
            _dictionary[key] = new[] { value };
        }
    }
}

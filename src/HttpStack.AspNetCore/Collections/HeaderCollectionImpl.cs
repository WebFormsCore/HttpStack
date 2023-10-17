using System.Collections;
using System.Collections.Generic;
using HttpStack.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack.AspNetCore.Collections;

internal class HeaderCollectionImpl : IHeaderDictionary
{
    private Microsoft.AspNetCore.Http.IHeaderDictionary _headerDictionary = null!;

    public void SetHeaderDictionary(Microsoft.AspNetCore.Http.IHeaderDictionary headerDictionary)
    {
        _headerDictionary = headerDictionary;
    }

    public void Reset()
    {
        _headerDictionary = null!;
    }

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return _headerDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_headerDictionary).GetEnumerator();
    }

    public void Add(KeyValuePair<string, StringValues> item)
    {
        _headerDictionary.Add(item);
    }

    public void Clear()
    {
        _headerDictionary.Clear();
    }

    public bool Contains(KeyValuePair<string, StringValues> item)
    {
        return _headerDictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
    {
        _headerDictionary.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, StringValues> item)
    {
        return _headerDictionary.Remove(item);
    }

    public int Count => _headerDictionary.Count;

    public bool IsReadOnly => _headerDictionary.IsReadOnly;

    public void Add(string key, StringValues value)
    {
        #pragma warning disable ASP0019
        _headerDictionary.Add(key, value);
        #pragma warning restore ASP0019
    }

    public bool ContainsKey(string key)
    {
        return _headerDictionary.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return _headerDictionary.Remove(key);
    }

    public bool TryGetValue(string key, out StringValues value)
    {
        return _headerDictionary.TryGetValue(key, out value);
    }

    public StringValues this[string key]
    {
        get => _headerDictionary[key];
        set => _headerDictionary[key] = value;
    }

    public ICollection<string> Keys => _headerDictionary.Keys;

    public ICollection<StringValues> Values => _headerDictionary.Values;

    public string? ContentType
    {
        get => _headerDictionary.ContentType;
        set => _headerDictionary.ContentType = value;
    }

    public long? ContentLength
    {
        get => _headerDictionary.ContentLength;
        set => _headerDictionary.ContentLength = value;
    }
}

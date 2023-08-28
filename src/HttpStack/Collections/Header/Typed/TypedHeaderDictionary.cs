using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Collections;

public class TypedHeaderDictionary : BaseHeaderDictionary
{
    private readonly IDictionary<string, StringValues> _dictionary;

    protected TypedHeaderDictionary(IDictionary<string, StringValues> dictionary)
    {
        _dictionary = dictionary;
    }

    public override IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    public override void Clear()
    {
        _dictionary.Clear();
    }

    public override int Count => _dictionary.Count;

    public override bool IsReadOnly => _dictionary.IsReadOnly;

    public override void Add(string key, StringValues value)
    {
        _dictionary.Add(key, value);
    }

    public override bool ContainsKey(string key)
    {
        return _dictionary.ContainsKey(key);
    }

    public override bool Remove(string key)
    {
        return _dictionary.Remove(key);
    }

    public override bool TryGetValue(string key, out StringValues value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public override ICollection<string> Keys => _dictionary.Keys;

    public override ICollection<StringValues> Values => _dictionary.Values;
}

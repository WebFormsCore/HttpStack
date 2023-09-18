using System.Collections.Generic;
using System.Linq;
using HttpStack.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Owin.Collections;

internal class OwinHeaderDictionary : BaseHeaderDictionary
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

    public override IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return _dictionary
            .Select(kv => new KeyValuePair<string, StringValues>(kv.Key, new StringValues(kv.Value)))
            .GetEnumerator();
    }

    public override void Add(KeyValuePair<string, StringValues> item)
    {
        _dictionary[item.Key] = item.Value.ToArray()!;
    }

    public override void Clear()
    {
        _dictionary.Clear();
    }

    public override int Count => _dictionary.Count;
    
    public override bool IsReadOnly => false;

    public override bool ContainsKey(string key)
    {
        return _dictionary.ContainsKey(key);
    }

    public override void Add(string key, StringValues value)
    {
        _dictionary.Add(key, value.ToArray()!);
    }

    public override bool Remove(string key)
    {
        if (ContainsKey(key))
        {
            _dictionary.Remove(key);
            return true;
        }

        return false;
    }

    public override bool TryGetValue(string key, out StringValues value)
    {
        if (ContainsKey(key))
        {
            value = _dictionary[key];
            return true;
        }

        value = default;
        return false;
    }

    public override ICollection<string> Keys => _dictionary.Keys;

    public override ICollection<StringValues> Values => _dictionary.Values
        .Select(v => new StringValues(v))
        .ToList();
}

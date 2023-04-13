using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Collections;

public class FormCollection : IFormCollection
{
    private readonly Dictionary<string, StringValues> _values = new();

    public void Reset()
    {
        _values.Clear();
        Files.Reset();
    }

    public void Add(string key, string value)
    {
        if (_values.TryGetValue(key, out var current))
        {
            _values[key] = current.Count switch
            {
                0 => new StringValues(value),
                1 => new StringValues(new[] { current[0], value }),
                _ => new StringValues(current.Append(value).ToArray())
            };
        }
        else
        {
            _values.Add(key, new StringValues(value));
        }
    }

    public void Add(NameValueCollection collection)
    {
        foreach (var key in collection.AllKeys)
        {
            Add(key!, collection[key]!);
        }
    }

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_values).GetEnumerator();
    }

    public int Count => _values.Count;

    public bool ContainsKey(string key)
    {
        return _values.ContainsKey(key);
    }

    public bool TryGetValue(string key, out StringValues value)
    {
        return _values.TryGetValue(key, out value);
    }

    public StringValues this[string key] => _values.TryGetValue(key, out var value) ? value : StringValues.Empty;

    public IEnumerable<string> Keys => _values.Keys;

    public IEnumerable<StringValues> Values => _values.Values;

    public FormFileCollection Files { get; } = new FormFileCollection();

    IFormFileCollection IFormCollection.Files => Files;
}

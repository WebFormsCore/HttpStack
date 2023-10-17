using System.Collections.Generic;
using System.Linq;
using HttpStack.Collections;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Azure.Functions.Collections;

internal class AzureHeaderDictionary : BaseHeaderDictionary
{
    private HttpHeadersCollection _header = null!;

    public void SetHttpHeaders(HttpHeadersCollection header)
    {
        _header = header;
    }

    public void Reset()
    {
        _header = null!;
    }

    public override IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return _header
            .Select(x => new KeyValuePair<string, StringValues>(x.Key, new StringValues(x.Value as string[] ?? x.Value.ToArray())))
            .GetEnumerator();
    }

    public override void Clear()
    {
        _header.Clear();
    }

    public override int Count => _header.Count();

    public override bool IsReadOnly => false;

    public override void Add(string key, StringValues value)
    {
        if (value.Count == 1)
        {
            _header.Add(key, value.ToString());
        }
        else
        {
            _header.Add(key, value.ToArray());
        }
    }

    public override bool ContainsKey(string key)
    {
        return _header.Contains(key);
    }

    public override bool Remove(string key)
    {
        return _header.Remove(key);
    }

    public override bool TryGetValue(string key, out StringValues value)
    {
        if (_header.Contains(key))
        {
            var enumerable = _header.GetValues(key);

            value = enumerable is string[] array
                ? new StringValues(array)
                : new StringValues(enumerable.ToArray());

            return true;
        }

        value = StringValues.Empty;
        return false;
    }

    public override ICollection<string> Keys => _header.Select(x => x.Key).ToArray();
    public override ICollection<StringValues> Values => _header.Select(x => new StringValues(x.Value as string[] ?? x.Value.ToArray())).ToArray();
}

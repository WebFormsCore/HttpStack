using System.Collections;
using System.Collections.Generic;

namespace HttpStack.AspNetCore.Collections;

internal class RequestCookiesImpl : IRequestCookieCollection
{
    private Microsoft.AspNetCore.Http.IRequestCookieCollection _requestCookieCollection = null!;

    public void SetRequestCookieCollection(Microsoft.AspNetCore.Http.IRequestCookieCollection requestCookieCollection)
    {
        _requestCookieCollection = requestCookieCollection;
    }

    public void Reset()
    {
        _requestCookieCollection = null!;
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _requestCookieCollection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _requestCookieCollection.Count;

    public ICollection<string> Keys => _requestCookieCollection.Keys;

    public bool ContainsKey(string key)
    {
        return _requestCookieCollection.ContainsKey(key);
    }

    public bool TryGetValue(string key, out string? value)
    {
        return _requestCookieCollection.TryGetValue(key, out value);
    }

    public string? this[string key] => _requestCookieCollection[key];
}

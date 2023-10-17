using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace HttpStack.NetHttpListener.Collections;

internal class RequestCookiesImpl : IRequestCookieCollection
{
    private CookieCollection _cookieCollection = null!;
    private readonly KeyCollection _keys;

    public RequestCookiesImpl()
    {
        _keys = new KeyCollection(this);
    }

    public void SetCookieCollection(CookieCollection httpCookieCollection)
    {
        _cookieCollection = httpCookieCollection;
    }

    public void Reset()
    {
        _cookieCollection = null!;
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (string key in _cookieCollection)
        {
            yield return new KeyValuePair<string, string>(key, _cookieCollection[key]?.Value ?? "");
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _cookieCollection.Count;
    public ICollection<string> Keys => _keys;
    public bool ContainsKey(string key)
    {
        return _cookieCollection[key] != null;
    }

    public bool TryGetValue(string key, out string? value)
    {
        value = _cookieCollection[key]?.Value;
        return value != null;
    }

    public string? this[string key] => _cookieCollection[key]?.Value;


    private class KeyCollection : ICollection<string>
    {
        private readonly RequestCookiesImpl _requestCookies;

        public KeyCollection(RequestCookiesImpl requestCookies)
        {
            _requestCookies = requestCookies;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _requestCookies._cookieCollection
                .Cast<Cookie>()
                .Select(c => c.Name)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(string item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Contains(string item) => _requestCookies.ContainsKey(item);

        public void CopyTo(string[] array, int arrayIndex)
        {
            foreach (Cookie cookie in _requestCookies._cookieCollection)
            {
                array[arrayIndex++] = cookie.Name;
            }
        }

        public bool Remove(string item)
        {
            throw new NotSupportedException();
        }

        public int Count => _requestCookies._cookieCollection.Count;
        public bool IsReadOnly => true;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Worker.Http;

namespace HttpStack.Azure.Functions.Collections;

internal class RequestCookiesImpl : IRequestCookieCollection
{
    private IReadOnlyCollection<IHttpCookie> _cookies = null!;
    private readonly KeyCollection _keys;

    public RequestCookiesImpl()
    {
        _keys = new KeyCollection(this);
    }

    public void SetCookies(IReadOnlyCollection<IHttpCookie> cookies)
    {
        _cookies = cookies;
    }

    public void Reset()
    {
        _cookies = null!;
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var cookie in _cookies)
        {
            yield return new KeyValuePair<string, string>(cookie.Name, cookie.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _cookies.Count;

    public ICollection<string> Keys  => _keys;

    public bool ContainsKey(string key)
    {
        foreach (var cookie in _cookies)
        {
            if (cookie.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetValue(string key, out string? value)
    {
        foreach (var cookie in _cookies)
        {
            if (cookie.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                value = cookie.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    public string? this[string key] => TryGetValue(key, out var value) ? value : null;

    private class KeyCollection : ICollection<string>
    {
        private readonly RequestCookiesImpl _requestCookies;

        public KeyCollection(RequestCookiesImpl requestCookies)
        {
            _requestCookies = requestCookies;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _requestCookies._cookies.Select(c => c.Name).GetEnumerator();
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
            foreach (var cookie in _requestCookies._cookies)
            {
                array[arrayIndex++] = cookie.Name;
            }
        }

        public bool Remove(string item)
        {
            throw new NotSupportedException();
        }

        public int Count => _requestCookies._cookies.Count;
        public bool IsReadOnly => true;
    }
}

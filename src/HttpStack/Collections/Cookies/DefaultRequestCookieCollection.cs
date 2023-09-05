using System;
using System.Collections;
using System.Collections.Generic;

namespace HttpStack.Collections.Cookies;

public class DefaultRequestCookieCollection : IRequestCookieCollection
{
    private const int MaxCookies = 300;
    private const int MaxCookieSize = 4096;

    private readonly IHttpRequest _request;
    private readonly Dictionary<string, string> _cookies = new();
    private bool _isLoaded;

    public DefaultRequestCookieCollection(IHttpRequest request)
    {
        _request = request;
    }

    public void Reset()
    {
        _isLoaded = false;
        _cookies.Clear();
    }

    private void Load()
    {
        if (_isLoaded)
        {
            return;
        }

        foreach (var cookie in _request.Headers["Cookie"])
        {
#if NET
            Load(cookie);
#else
            Load(cookie.AsSpan());
#endif
        }
    }

    private void Load(ReadOnlySpan<char> cookieHeader)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;

        if (cookieHeader.IsEmpty)
        {
            return;
        }

        while (true)
        {
            var index = cookieHeader.IndexOf(';');

            if (index == -1)
            {
                AddPart(cookieHeader);
                break;
            }

            AddPart(cookieHeader.Slice(0, index));
        }
    }

    private void AddPart(ReadOnlySpan<char> slice)
    {
        if (_cookies.Count >= MaxCookies)
        {
            throw new InvalidOperationException("Too many cookies.");
        }

        if (slice.Length > MaxCookieSize)
        {
            throw new InvalidOperationException("Cookie too large.");
        }

        var index = slice.IndexOf('=');
        string key;
        string value;

        if (index == -1)
        {
            key = slice.Trim().ToString();
            value = string.Empty;
        }
        else
        {
            key = slice.Slice(0, index).Trim().ToString();
            value = slice.Slice(index + 1).Trim().ToString();
        }

        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        _cookies.Add(key, value);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        Load();
        return _cookies.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count
    {
        get
        {
            Load();
            return _cookies.Count;
        }
    }

    public ICollection<string> Keys
    {
        get
        {
            Load();
            return _cookies.Keys;
        }
    }

    public bool ContainsKey(string key)
    {
        Load();
        return _cookies.ContainsKey(key);
    }

    public bool TryGetValue(string key, out string? value)
    {
        Load();
        return _cookies.TryGetValue(key, out value);
    }

    public string? this[string key]
    {
        get
        {
            Load();
            return _cookies[key];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;

namespace HttpStack.NetHttpListener.Collections;

internal class ResponseCookiesImpl : IResponseCookies
{
    private static readonly CookieOptions DefaultOptions = new();
    private CookieCollection _cookieCollection = null!;

    public void SetCookieCollection(CookieCollection httpCookieCollection)
    {
        _cookieCollection = httpCookieCollection;
    }

    public void Reset()
    {
        _cookieCollection = null!;
    }

    public void Append(string key, string value)
    {
        var cookie = GetOrCreateCookie(key, value);
        UpdateCookie(cookie, DefaultOptions);
    }

    public void Append(string key, string value, CookieOptions options)
    {
        var cookie = GetOrCreateCookie(key, value);
        UpdateCookie(cookie, options);
    }

    public void Append(ReadOnlySpan<KeyValuePair<string, string>> keyValuePairs, CookieOptions options)
    {
        foreach (var keyValuePair in keyValuePairs)
        {
            Append(keyValuePair.Key, keyValuePair.Value, options);
        }
    }

    public void Delete(string key)
    {
        var cookie = GetOrCreateCookie(key, "");
        UpdateCookie(cookie, DefaultOptions);
        cookie.Expires = DateTime.Now.AddDays(-1);
    }

    public void Delete(string key, CookieOptions options)
    {
        var cookie = GetOrCreateCookie(key, "");
        UpdateCookie(cookie, options);
        cookie.Expires = DateTime.Now.AddDays(-1);
    }

    private Cookie GetOrCreateCookie(string key, string value)
    {
        var cookie = _cookieCollection[key];

        if (cookie != null)
        {
            cookie.Value = value;
            return cookie;
        }

        var newCookie = new Cookie(key, value);
        _cookieCollection.Add(newCookie);
        return newCookie;
    }

    private static void UpdateCookie(Cookie cookie, CookieOptions options)
    {
        cookie.Domain = options.Domain;
        cookie.Path = options.Path;
        cookie.Secure = options.Secure;
        cookie.HttpOnly = options.HttpOnly;
        cookie.Expires = options.Expires?.UtcDateTime ?? DateTime.MinValue;
    }
}

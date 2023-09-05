using System;
using System.Collections.Generic;
using System.Web;

namespace HttpStack.AspNet.Collections;

internal class ResponseCookiesImpl : IResponseCookies
{
    private static readonly CookieOptions DefaultOptions = new();
    private HttpCookieCollection _httpCookieCollection = null!;

    public void SetHttpCookieCollection(HttpCookieCollection httpCookieCollection)
    {
        _httpCookieCollection = httpCookieCollection;
    }

    public void Reset()
    {
        _httpCookieCollection = null!;
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

    private HttpCookie GetOrCreateCookie(string key, string value)
    {
        var cookie = _httpCookieCollection[key];

        if (cookie != null)
        {
            cookie.Value = value;
            return cookie;
        }

        var newCookie = new HttpCookie(key, value);
        _httpCookieCollection.Add(newCookie);
        return newCookie;
    }

    private static void UpdateCookie(HttpCookie cookie, CookieOptions options)
    {
        cookie.Expires = options.Expires?.UtcDateTime ?? DateTime.MinValue;
        cookie.Domain = options.Domain;
        cookie.Path = options.Path;
        cookie.Secure = options.Secure;
        cookie.HttpOnly = options.HttpOnly;
        cookie.SameSite = options.SameSite switch
        {
            SameSiteMode.Lax => System.Web.SameSiteMode.Lax,
            SameSiteMode.Strict => System.Web.SameSiteMode.Strict,
            SameSiteMode.None => System.Web.SameSiteMode.None,
            SameSiteMode.Unspecified => System.Web.SameSiteMode.Lax,
            _ => throw new ArgumentOutOfRangeException(nameof(options.SameSite), options.SameSite, null)
        };
    }
}

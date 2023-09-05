using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker.Http;

namespace HttpStack.Azure.Functions.Collections;

internal class ResponseCookiesImpl : IResponseCookies
{
    private HttpCookies _cookies = null!;

    public void SetCookies(HttpCookies cookies)
    {
        _cookies = cookies;
    }

    public void Reset()
    {
        _cookies = null!;
    }

    public void Append(string key, string value)
    {
        _cookies.Append(key, value);
    }

    public void Append(string key, string value, CookieOptions options)
    {
        _cookies.Append(ToAzureCookie(key, value, options));
    }

    public void Append(ReadOnlySpan<KeyValuePair<string, string>> keyValuePairs, CookieOptions options)
    {
        foreach (var keyValuePair in keyValuePairs)
        {
            _cookies.Append(ToAzureCookie(keyValuePair.Key, keyValuePair.Value, options));
        }
    }

    public void Delete(string key)
    {
        _cookies.Append(new HttpCookie(key, string.Empty)
        {
            Expires = DateTime.Now.AddDays(-1)
        });
    }

    public void Delete(string key, CookieOptions options)
    {
        var cookie = ToAzureCookie(key, string.Empty, options);
        cookie.Expires = DateTime.Now.AddDays(-1);
        _cookies.Append(cookie);
    }

    private static HttpCookie ToAzureCookie(string key, string value, CookieOptions options)
    {
        return new HttpCookie(key, value)
        {
            Path = options.Path,
            Domain = options.Domain,
            Expires = options.Expires,
            Secure = options.Secure,
            HttpOnly = options.HttpOnly,
            SameSite = options.SameSite switch {
                SameSiteMode.Unspecified => SameSite.Lax,
                SameSiteMode.None => SameSite.ExplicitNone,
                SameSiteMode.Lax => SameSite.Lax,
                SameSiteMode.Strict => SameSite.Strict,
                _ => throw new ArgumentOutOfRangeException()
            },
            MaxAge = options.MaxAge?.TotalSeconds
        };
    }
}

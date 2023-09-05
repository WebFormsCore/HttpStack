using System;
using System.Collections.Generic;

namespace HttpStack.AspNetCore.Collections;

internal class ResponseCookiesImpl : IResponseCookies
{
    private Microsoft.AspNetCore.Http.IResponseCookies _responseCookies = null!;

    public void SetResponseCookies(Microsoft.AspNetCore.Http.IResponseCookies responseCookies)
    {
        _responseCookies = responseCookies;
    }

    public void Reset()
    {
        _responseCookies = null!;
    }

    public void Append(string key, string value)
    {
        _responseCookies.Append(key, value);
    }

    public void Append(string key, string value, CookieOptions options)
    {
        _responseCookies.Append(key, value, ToAspNetCoreCookie(options));
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
        _responseCookies.Delete(key);
    }

    public void Delete(string key, CookieOptions options)
    {
        _responseCookies.Delete(key, ToAspNetCoreCookie(options));
    }

    private static Microsoft.AspNetCore.Http.CookieOptions ToAspNetCoreCookie(CookieOptions options)
    {
        return new Microsoft.AspNetCore.Http.CookieOptions
        {
            Domain = options.Domain,
            Path = options.Path,
            Secure = options.Secure,
            HttpOnly = options.HttpOnly,
            Expires = options.Expires,
            IsEssential = options.IsEssential,
            MaxAge = options.MaxAge,
            SameSite = options.SameSite switch
            {
                SameSiteMode.Lax => Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                SameSiteMode.Strict => Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                SameSiteMode.None => Microsoft.AspNetCore.Http.SameSiteMode.None,
                SameSiteMode.Unspecified => Microsoft.AspNetCore.Http.SameSiteMode.Unspecified,
                _ => throw new ArgumentOutOfRangeException(nameof(options.SameSite), options.SameSite, null)
            }
        };
    }
}

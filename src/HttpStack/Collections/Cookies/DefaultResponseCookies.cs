using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Collections.Cookies;

public class DefaultResponseCookies : IResponseCookies
{
    private readonly Dictionary<string, CookieValue> _cookies = new();
    private readonly IHttpResponse _response;

    public DefaultResponseCookies(IHttpResponse response)
    {
        _response = response;
    }

    public void Reset()
    {
        _cookies.Clear();
    }

    private void SetCookieHeader()
    {
        var encoder = UrlEncoder.Default;
        var values = new string[_cookies.Count];
        var sb = new StringBuilder();
        var index = 0;

        foreach (var cookie in _cookies)
        {
            sb.Clear();

            sb.Append(cookie.Key);
            sb.Append('=');
            sb.Append(cookie.Value.Value);

            var options = cookie.Value.Options;

            if (options is not null)
            {
                if (options.Expires != null)
                {
                    sb.Append("; Expires=");
                    sb.Append(options.Expires.Value.ToString("R"));
                }

                if (options.Domain != null)
                {
                    sb.Append("; Domain=");
                    sb.Append(options.Domain);
                }

                if (options.Path != null)
                {
                    sb.Append("; Path=");
                    sb.Append(options.Path);
                }

                if (options.MaxAge != null)
                {
                    sb.Append("; Max-Age=");
                    sb.Append(options.MaxAge.Value.TotalSeconds.ToString("0"));
                }

                if (options.HttpOnly)
                {
                    sb.Append("; HttpOnly");
                }

                if (options.Secure)
                {
                    sb.Append("; Secure");
                }

                if (options.SameSite != SameSiteMode.Unspecified)
                {
                    sb.Append("; SameSite=");
                    sb.Append(options.SameSite switch
                    {
                        SameSiteMode.Lax => "Lax",
                        SameSiteMode.Strict => "Strict",
                        SameSiteMode.None => "None",
                        _ => throw new InvalidOperationException("Invalid enum value.")
                    });
                }
            }

            values[index++] = encoder.Encode(sb.ToString());
        }

        _response.Headers["Set-Cookie"] = new StringValues(values);
    }


    public void Append(string key, string value)
    {
        _cookies[key] = new CookieValue(value);
        SetCookieHeader();
    }

    public void Append(string key, string value, CookieOptions options)
    {
        _cookies[key] = new CookieValue(value, options);
        SetCookieHeader();
    }

    public void Append(ReadOnlySpan<KeyValuePair<string, string>> keyValuePairs, CookieOptions options)
    {
        foreach (var keyValuePair in keyValuePairs)
        {
            _cookies[keyValuePair.Key] = new CookieValue(keyValuePair.Value, options);
        }

        SetCookieHeader();
    }

    public void Delete(string key)
    {
        _cookies[key] = new CookieValue(string.Empty, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(-1)
        });

        SetCookieHeader();
    }

    public void Delete(string key, CookieOptions options)
    {
        options.Expires = DateTime.Now.AddDays(-1);
        _cookies[key] = new CookieValue(string.Empty, options);
        SetCookieHeader();
    }
}

internal record struct CookieValue(string Value, CookieOptions? Options = null);
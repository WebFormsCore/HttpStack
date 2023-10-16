using System;
using System.Collections.Generic;

namespace HttpStack.Collections.Cookies;

public class ResponseCookieCollection : IResponseCookies
{
	private readonly Dictionary<string, (string, CookieOptions?)> _dictionary = new();

	public void Append(string key, string value)
	{
		_dictionary[key] = (value, null);
	}

	public void Append(string key, string value, CookieOptions options)
	{
		_dictionary[key] = (value, options);
	}

	public void Append(ReadOnlySpan<KeyValuePair<string, string>> keyValuePairs, CookieOptions options)
	{
		foreach (var keyValuePair in keyValuePairs)
		{
			_dictionary[keyValuePair.Key] = (keyValuePair.Value, options);
		}
	}

	public void Delete(string key)
	{
		_dictionary.Remove(key);
	}

	public void Delete(string key, CookieOptions options)
	{
		_dictionary.Remove(key);
	}
}

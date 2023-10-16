using System.IO;
using HttpStack.Collections;

namespace HttpStack;

public interface IReadOnlyHttpRequest
{
	string Method { get; }

	string Scheme { get; }

	string? Host { get; }

	bool IsHttps { get; }

	string Protocol { get; }

	string? ContentType { get; }

	Stream Body { get; }

	PathString Path { get; }

	QueryString QueryString { get; }

	IQueryCollection Query { get; }

	IFormCollection Form { get; }

	IRequestHeaderDictionary Headers { get; }

	IRequestCookieCollection Cookies { get; }
}

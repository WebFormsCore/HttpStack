using System.IO;
using HttpStack.Collections;
using HttpStack.Collections.Cookies;

namespace HttpStack;

public class InMemoryHttpRequest : IHttpRequest
{
	public string Method { get; set; } = "GET";

	public string Scheme { get; set; } = "http";

	public string? Host { get; set; }

	public bool IsHttps { get; set; }

	public string Protocol { get; set; } = "HTTP/1.1";

	public string? ContentType { get; set; }

	public Stream Body { get; set; } = Stream.Null;

	public PathString Path { get; set; }
	public QueryString QueryString { get; set; }

	public IQueryCollection Query { get; set; } = new QueryCollection();


	public IFormCollection Form { get; set; } = new FormCollection();

	public IRequestHeaderDictionary Headers { get; set; } = new RequestHeaderDictionary();

	public IRequestCookieCollection Cookies { get; set; } = new RequestCookieCollection();
}

using System.IO;
using HttpStack.Collections;
using HttpStack.Collections.Cookies;

namespace HttpStack;

public class InMemoryHttpResponse : IHttpResponse
{
	public int StatusCode { get; set; }

	public string? ContentType { get; set; }

	public IResponseHeaderDictionary Headers { get; set; } = new ResponseHeaderDictionary();

	public IResponseCookies Cookies { get; set; } = new ResponseCookieCollection();

	public Stream Body { get; set; } = Stream.Null;

	public bool HasStarted { get; set; }

	public long? ContentLength { get; set; }
}

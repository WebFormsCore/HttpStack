using System.IO;
using HttpStack.Collections;

namespace HttpStack;

/// <summary>Lazily initialized <see cref="IHttpRequest"/> implementation.</summary>
/// <remarks>Only the properties that are accessed will be initialized from the underlying <see cref="IReadOnlyHttpRequest"/>.</remarks>
public class LazyHttpRequest : IHttpRequest
{
	private readonly IReadOnlyHttpRequest _request;
	private string? _method;
	private string? _scheme;
	private string? _host;
	private bool? _isHttps;
	private string? _protocol;
	private string? _contentType;
	private Stream? _body;
	private PathString? _path;
	private QueryString? _queryString;
	private IQueryCollection? _query;
	private IFormCollection? _form;
	private IRequestHeaderDictionary? _headers;
	private IRequestCookieCollection? _cookies;

	public LazyHttpRequest(IReadOnlyHttpRequest request)
	{
		_request = request;
	}

	public string Method
	{
		get => _method ??= _request.Method;
		set => _method = value;
	}

	public string Scheme
	{
		get => _scheme ??= _request.Scheme;
		set => _scheme = value;
	}

	public string? Host
	{
		get => _host ??= _request.Host;
		set => _host = value;
	}

	public bool IsHttps
	{
		get => _isHttps ??= _request.IsHttps;
		set => _isHttps = value;
	}

	public string Protocol
	{
		get => _protocol ??= _request.Protocol;
		set => _protocol = value;
	}

	public string? ContentType
	{
		get => _contentType ??= _request.ContentType;
		set => _contentType = value;
	}

	public Stream Body
	{
		get => _body ??= _request.Body;
		set => _body = value;
	}

	public PathString Path
	{
		get => _path ??= _request.Path;
		set => _path = value;
	}

	public QueryString QueryString
	{
		get => _queryString ??= _request.QueryString;
		set => _queryString = value;
	}

	public IQueryCollection Query
	{
		get => _query ??= _request.Query;
		set => _query = value;
	}

	public IFormCollection Form
	{
		get => _form ??= _request.Form;
		set => _form = value;
	}

	public IRequestHeaderDictionary Headers
	{
		get => _headers ??= _request.Headers;
		set => _headers = value;
	}

	public IRequestCookieCollection Cookies
	{
		get => _cookies ??= _request.Cookies;
		set => _cookies = value;
	}

	public void Reset()
	{
		_method = null;
		_scheme = null;
		_host = null;
		_isHttps = null;
		_protocol = null;
		_contentType = null;
		_body = null;
		_path = null;
		_queryString = null;
		_query = null;
		_form = null;
		_headers = null;
		_cookies = null;
	}
}

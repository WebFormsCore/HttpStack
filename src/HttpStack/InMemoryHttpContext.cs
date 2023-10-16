using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using HttpStack.Collections;
using HttpStack.DependencyInjection;
using HttpStack.Http;

namespace HttpStack;

public class InMemoryHttpContext : IHttpContext
{
	public InMemoryHttpRequest Request { get; set; } = new();

	public InMemoryHttpResponse Response { get; set; } = new();

	public IDictionary<object, object?> Items { get; set; } = new Dictionary<object, object?>();

	public IServiceProvider RequestServices { get; set; } = DefaultServiceProvider.Instance;


	public CancellationToken RequestAborted { get; set; } = CancellationToken.None;

	public IFeatureCollection Features { get; set; } = new DefaultFeatureCollection();

	public WebSocketManager WebSockets { get; set; } = DefaultWebSocketManagerImp.Instance;

	public ClaimsPrincipal User { get; set; } = new();

	public ISession Session { get; set; } = new DefaultSession();

	IHttpRequest IHttpContext.Request => Request;
	IHttpResponse IHttpContext.Response => Response;
}

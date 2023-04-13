using System.Threading.Tasks;

namespace HttpStack;

public delegate Task MiddlewareDelegate(IHttpContext context);

using System.Threading.Tasks;

namespace HttpStack;

public interface IMiddleware
{
    Task Invoke(IHttpContext context, MiddlewareDelegate next);
}

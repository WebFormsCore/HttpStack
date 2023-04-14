using System.Web;

namespace HttpStack.AspNet;

internal class VoidHandler : IHttpHandler
{
    public static readonly VoidHandler Instance = new();

    public void ProcessRequest(HttpContext context)
    {
        // ignore
    }

    public bool IsReusable => true;
}
